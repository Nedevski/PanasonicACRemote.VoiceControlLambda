using System;
using System.Collections.Generic;
using System.Text;

namespace PanasonicACVoiceRemote.Model
{
    public enum Power
    {
        NotSet,
        Off,
        On
    }

    public enum Swing
    {
        NotSet,
        High,
        SemiHigh,
        Middle,
        SemiLow,
        Low,
        Auto
    }

    public enum Modifier
    {
        NotSet,
        Quiet,
        Powerful,
        Normal
    }

    public enum Mode
    {
        NotSet,
        Hot,
        Cold,
        Dry,
        Auto
    }

    public enum Fan
    {
        NotSet,
        One,
        Two,
        Three,
        Four,
        Five,
        Auto
    }
}
