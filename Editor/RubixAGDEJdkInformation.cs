using System;

namespace Rubix.Android.VisualStudio.AGDE.Editor
{
    class JdkInformation
    {
        public enum JdkState
        {
            Success,
            Warning,
            Error
        }

        public Version Version { get; }

        public JdkState State { get; }

        public string Message { get; }

        public JdkInformation(Version version, JdkState state, string message)
        {
            Version = version;
            State = state;
            Message = message;
        }

        public static readonly Version MinAGDEJdkVersion = new Version(17, 0, 9);
    }
}
