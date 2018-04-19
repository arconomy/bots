using System;
using Niffler.Messaging.Protobuf;
using cAlgo.API;
using Niffler.Core.Config;
using Newtonsoft.Json;

namespace Niffler.Common
{
    public class Utils
    {
        private static DateTime GetDateTime(Tick tick)
        {
            return DateTime.FromBinary(tick.TimeStamp);
        }

        public static string GetTimeStamp(bool formatWithSeperators = false)
        {
            DateTime datetime = System.DateTime.Now;

            if (formatWithSeperators)
                return FormatDateTimeWithSeparators(datetime);

            return FormatDateTime(datetime);
        }

        public static string GetTimeStamp(Tick tick, bool formatWithSeperators = false)
        {
            DateTime datetime = GetDateTime(tick);

            if (formatWithSeperators)
                return FormatDateTimeWithSeparators(datetime);

            return FormatDateTime(datetime);
        }

        public static string FormatDateTime(DateTime datetime)
        {
            return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Hour.ToString("00") + datetime.Minute.ToString("00") + datetime.Second.ToString("00");
        }

        public static string FormatDateTimeWithSeparators(long datetime)
        {
            return FormatDateTimeWithSeparators(DateTime.FromBinary(datetime));
        }

            public static string FormatDateTimeWithSeparators(DateTime datetime)
        {
            return datetime.Day + "-" + datetime.Month + "-" + datetime.Year.ToString() + " " + datetime.Hour.ToString("00") + ":" + datetime.Minute.ToString("00") + ":" + datetime.Second.ToString("00");
        }

        public static string GetUniqueID()
        {
            DateTime datetime = System.DateTime.Now;
            return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Hour + datetime.Minute + datetime.Second;
        }
        
        public static string GetStrategyId(string Label)
        {
            return Label.Substring(0, 5);
        }

        public static double GetMidPrice(Tick tick)
        {
            return tick.Bid + tick.Spread / 2;
        }

        public static bool GetRuleConfigIntegerParam(String ruleConfigParamName, RuleConfiguration ruleConfig, ref int intValue)
        {
            if (ruleConfig.Params.TryGetValue(ruleConfigParamName, out object paramIntObj))
            {
                if (!Int32.TryParse(paramIntObj.ToString(), out intValue))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool GetRuleConfigDoubleParam(String ruleConfigParamName, RuleConfiguration ruleConfig, ref double doubleValue)
        {
            if (ruleConfig.Params.TryGetValue(ruleConfigParamName, out object paramDoubleObj))
            {
                if (!Double.TryParse(paramDoubleObj.ToString(), out doubleValue))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GetRuleConfigBoolParam(String ruleConfigParamName, RuleConfiguration ruleConfig, ref bool boolValue)
        {
            boolValue = false;
            if (ruleConfig.Params.TryGetValue(ruleConfigParamName, out object paramBoolObj))
            {
                if (!Boolean.TryParse(paramBoolObj.ToString(), out boolValue))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GetRuleConfigStringParam(String ruleConfigParamName, RuleConfiguration ruleConfig, ref string stringValue)
        {
            if (!ruleConfig.Params.TryGetValue(ruleConfigParamName, out object paramStringObj)) return false;
            stringValue = paramStringObj.ToString();
            return true;
        }

        public static bool GetRuleConfigOrderSpacing(RuleConfiguration ruleConfig, ref OrderSpacingConfiguration orderSpacingConfig)
        {
            if (ruleConfig.Params.TryGetValue(RuleConfiguration.ORDERSPACING, out object paramObj))
            {
                orderSpacingConfig = JsonConvert.DeserializeObject<OrderSpacingConfiguration>(paramObj.ToString());

                if (orderSpacingConfig == null)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GetRuleConfigVolume(RuleConfiguration ruleConfig, ref VolumeConfiguration VolumeConfig)
        {
            if (ruleConfig.Params.TryGetValue(RuleConfiguration.VOLUME, out object paramObj))
            {
                VolumeConfig = JsonConvert.DeserializeObject<VolumeConfiguration>(paramObj.ToString());

                if (VolumeConfig == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
