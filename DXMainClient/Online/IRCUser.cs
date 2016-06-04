﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTAClient.Online
{
    public class IRCUser
    {
        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        int _gameId = -1;

        public int GameID
        {
            get { return _gameId; }
            set { _gameId = value; }
        }
    }
}
