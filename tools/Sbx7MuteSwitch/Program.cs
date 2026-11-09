using System;
using System.Threading;
using CTSoundCore;

namespace Sbx7MuteSwitch
{
    internal static class Program
    {
        private const int DefaultHoldMs = 1000;

        private static int Main(string[] args)
        {
            string mode = null;
            string value = null;
            int holdMs = DefaultHoldMs;
            bool dryRun = false;
            bool restoreMute = true;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--mode":
                        mode = RequireValue(args, ref i);
                        break;
                    case "--value":
                        value = RequireValue(args, ref i);
                        break;
                    case "--hold-ms":
                        holdMs = ParseHold(RequireValue(args, ref i));
                        if (holdMs < 0)
                        {
                            Console.Error.WriteLine("--hold-ms must be zero or positive.");
                            return 2;
                        }
                        break;
                    case "--dry-run":
                        dryRun = true;
                        break;
                    case "--no-restore":
                        restoreMute = false;
                        break;
                    case "-h":
                    case "--help":
                        PrintHelp();
                        return 0;
                    default:
                        Console.Error.WriteLine("Unknown argument: " + args[i]);
                        PrintHelp();
                        return 2;
                }
            }

            if (mode == null || value == null)
            {
                PrintHelp();
                return 2;
            }

            bool enable;
            if (!TryParseOnOff(value, out enable))
            {
                Console.Error.WriteLine("--value must be on or off.");
                return 2;
            }

            if (!IsKnownMode(mode))
            {
                Console.Error.WriteLine("Unknown mode: " + mode);
                PrintHelp();
                return 2;
            }

            if (dryRun)
            {
                PrintPlan(mode, enable, holdMs, restoreMute);
                return 0;
            }

            try
            {
                ISoundCore core = new CtHdaMgrClass();
                _etContext context;
                core.GetContext(out context);
                if (context == _etContext.eContext_Invalid)
                {
                    context = _etContext.eContext_Standard;
                }

                Console.WriteLine("Context: " + context);
                bool initialMute = GetMute(core, context);
                Console.WriteLine("Initial mute: " + initialMute);
                Console.WriteLine("Muting, waiting " + holdMs + " ms...");

                bool muted = false;
                try
                {
                    SetMute(core, context, true);
                    muted = true;
                    Thread.Sleep(holdMs);

                    ApplyMode(core, context, mode, enable);
                    Console.WriteLine("Mode " + mode + " -> " + (enable ? "on" : "off") + "; waiting " + holdMs + " ms...");
                    Thread.Sleep(holdMs);
                }
                finally
                {
                    if (restoreMute && muted)
                    {
                        SetMute(core, context, initialMute);
                        Console.WriteLine("Mute restored to " + initialMute + ".");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed: " + ex.Message);
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void ApplyMode(ISoundCore core, _etContext context, string mode, bool enable)
        {
            switch (mode)
            {
                case "direct":
                    SetProcessingDword(core, context,
                        _etParamProcessingControl.eParamProcessingControl_DirectMode,
                        enable ? 1u : 0u);
                    break;
                case "stereo-direct":
                    SetProcessingDword(core, context,
                        _etParamProcessingControl.eParamProcessingControl_StereoDirectMode,
                        enable ? 1u : 0u);
                    break;
                case "sbxmaster":
                    SetEfxMasterBool(core, context,
                        _etParamEfxMasterControl.eParamEfxMasterControl_SBXMasterOnOff,
                        enable);
                    break;
                default:
                    throw new ArgumentException("Unknown mode: " + mode);
            }
        }

        private static bool GetMute(ISoundCore core, _etContext context)
        {
            var param = new _stParam
            {
                featureId = _etFeature.eFeature_Effects_SpeakerCalibrator,
                contextId = context
            };
            param.paramId.paramIdSpeakerCalibrator = _etParamSpeakerCalibrator.eParamSpeakerCalibrator_Mute;

            _stParamValue value;
            core.GetParamValue(param, out value);
            if (value.paramType != _etParamType.eParamType_Bool)
            {
                throw new InvalidOperationException("SpeakerCalibrator_Mute is not a bool parameter.");
            }

            return value.paramVal.boolVal != 0;
        }

        private static void SetMute(ISoundCore core, _etContext context, bool mute)
        {
            var param = new _stParam
            {
                featureId = _etFeature.eFeature_Effects_SpeakerCalibrator,
                contextId = context
            };
            param.paramId.paramIdSpeakerCalibrator = _etParamSpeakerCalibrator.eParamSpeakerCalibrator_Mute;

            var value = new _stParamValue
            {
                paramType = _etParamType.eParamType_Bool
            };
            value.paramVal.boolVal = mute ? 1 : 0;
            core.SetParamValue(param, value);
        }

        private static void SetProcessingDword(ISoundCore core, _etContext context,
            _etParamProcessingControl parameter, uint value)
        {
            var param = new _stParam
            {
                featureId = _etFeature.eFeature_System_ProcessingControl,
                contextId = context
            };
            param.paramId.paramIdProcessingControl = parameter;

            var paramValue = new _stParamValue
            {
                paramType = _etParamType.eParamType_Dword
            };
            paramValue.paramVal.dwordVal = value;
            core.SetParamValue(param, paramValue);
        }

        private static void SetEfxMasterBool(ISoundCore core, _etContext context,
            _etParamEfxMasterControl parameter, bool value)
        {
            var param = new _stParam
            {
                featureId = _etFeature.eFeature_EfxMasterControl,
                contextId = context
            };
            param.paramId.paramIdEfxMasterControl = parameter;

            var paramValue = new _stParamValue
            {
                paramType = _etParamType.eParamType_Bool
            };
            paramValue.paramVal.boolVal = value ? 1 : 0;
            core.SetParamValue(param, paramValue);
        }

        private static string RequireValue(string[] args, ref int index)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException("Missing value for " + args[index]);
            }

            index++;
            return args[index];
        }

        private static int ParseHold(string text)
        {
            int value;
            if (!int.TryParse(text, out value))
            {
                throw new ArgumentException("--hold-ms must be an integer.");
            }

            return value;
        }

        private static bool TryParseOnOff(string text, out bool value)
        {
            if (string.Equals(text, "on", StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }
            if (string.Equals(text, "off", StringComparison.OrdinalIgnoreCase))
            {
                value = false;
                return true;
            }

            value = false;
            return false;
        }

        private static bool IsKnownMode(string mode)
        {
            return mode == "direct" || mode == "stereo-direct" || mode == "sbxmaster";
        }

        private static void PrintPlan(string mode, bool enable, int holdMs, bool restoreMute)
        {
            Console.WriteLine("Dry run; no COM calls.");
            Console.WriteLine("Sequence: read mute -> mute on -> wait " + holdMs + " ms -> set " + mode + " " + (enable ? "on" : "off") + " -> wait " + holdMs + " ms -> " + (restoreMute ? "restore previous mute" : "leave muted"));
            Console.WriteLine("Mute parameter: feature=0x" + ((int)_etFeature.eFeature_Effects_SpeakerCalibrator).ToString("X8") + " param=" + (int)_etParamSpeakerCalibrator.eParamSpeakerCalibrator_Mute);
            Console.WriteLine("Direct mode: feature=0x" + ((int)_etFeature.eFeature_System_ProcessingControl).ToString("X8") + " param=" + (int)_etParamProcessingControl.eParamProcessingControl_DirectMode);
            Console.WriteLine("Stereo direct mode: feature=0x" + ((int)_etFeature.eFeature_System_ProcessingControl).ToString("X8") + " param=" + (int)_etParamProcessingControl.eParamProcessingControl_StereoDirectMode);
            Console.WriteLine("SBX master: feature=0x" + ((int)_etFeature.eFeature_EfxMasterControl).ToString("X8") + " param=" + (int)_etParamEfxMasterControl.eParamEfxMasterControl_SBXMasterOnOff);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Sbx7MuteSwitch - mute the Sound Blaster X7 around a mode switch.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Sbx7MuteSwitch.exe --mode direct --value off [--hold-ms 1000]");
            Console.WriteLine("  Sbx7MuteSwitch.exe --mode stereo-direct --value on");
            Console.WriteLine("  Sbx7MuteSwitch.exe --mode sbxmaster --value off");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --mode direct|stereo-direct|sbxmaster");
            Console.WriteLine("  --value on|off");
            Console.WriteLine("  --hold-ms N        Wait N ms after muting and after the switch (default 1000).");
            Console.WriteLine("  --dry-run          Print the parameter plan without calling COM.");
            Console.WriteLine("  --no-restore       Leave the device muted after the switch.");
            Console.WriteLine();
            Console.WriteLine("Requires the installed Sound Blaster X7 control panel and its COM server.");
            Console.WriteLine("This tool does not modify firmware. Keep the amplifier volume low for the first test.");
        }
    }
}