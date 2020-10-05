using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bonsai
{
    [BTNode("Tasks/", "Editor_Log")]
    public class Print : Task
    {
        public enum ELogType : byte { Normal, Warning, Error }

        [TextArea]
        public string message = "Print Node";

        [Tooltip("the type of message to display.")]
        public ELogType logType = ELogType.Normal;

        public override EStatus Run()
        {
            switch (logType)
            {
                case ELogType.Normal:
                    Debug.Log(message);
                    break;
                case ELogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case ELogType.Error:
                    Debug.LogError(message);
                    break;
                default:
                    break;
            }

            return EStatus.Success;
        }

        public override void Description(StringBuilder builder)
        {
            // Nothing to display.
            if (message.Length == 0)
            {
                return;
            }

            string displayed = message;

            // Only consider display the message up to the newline.
            int newLineIndex = message.IndexOf('\n');
            if (newLineIndex >= 0)
            {
                displayed = message.Substring(0, newLineIndex);
            }

            // Nothing to display.
            if (displayed.Length == 0)
            {
                return;
            }

            if (logType != ELogType.Normal)
            {
                builder.AppendLine(logType.ToString());
            }

            // Cap the message length to display to keep things compact.
            int maxCharacters = 20;
            if (displayed.Length > maxCharacters)
            {
                builder.Append(displayed.Substring(0, maxCharacters));
                builder.Append("...");
            }
            else
            {
                builder.Append(displayed);
            }
        }
    }

}