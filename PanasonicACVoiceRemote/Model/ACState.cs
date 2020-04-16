﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PanasonicACVoiceRemote.Model
{
    public class ACState
    {
        public Power Power { get; set; }
        public SwingPosition Swing { get; set; }
        public Modifier Modifiers { get; set; }
        public Mode Mode { get; set; }
        public int Temp { get; set; }

        public StringContent AsJson()
        {
            return new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8);
        }
    }
}