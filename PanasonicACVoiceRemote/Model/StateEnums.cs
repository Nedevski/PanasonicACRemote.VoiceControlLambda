using System;
using System.Collections.Generic;
using System.Text;

namespace PanasonicACVoiceRemote.Model
{
    public enum Power
    {
        NotSet,
        On,
        Off
    }

    public enum Mode
    {
        NotSet,
        Auto,
        Hot,
        Cold,
        Dry
    }

    public enum Fan
    {
        NotSet,
        Auto,
        One,
        Two,
        Three,
        Four,
        Five,
    }
    
    public enum Swing
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
        Normal,
        Quiet,
        Powerful
    }
}
