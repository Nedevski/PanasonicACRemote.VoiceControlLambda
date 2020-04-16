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

    public enum SwingPosition
    {
        NotSet,
        Auto,
        High,
        SemiHigh,
        Middle,
        SemiLow,
        Low
    }

    public enum Modifier
    {
        NotSet,
        Auto,
        Quiet,
        Powerful
    }

    public enum Mode
    {
        NotSet,
        Auto,
        Hot,
        Cold,
        Dry
    }
}
