/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Player.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4  

    AGMGSKv7 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//#if MONOGAMES //  true, build for MonoGames
//   using Microsoft.Xna.Framework.Storage; 
//#endif
#endregion

namespace AGMGSKv7
{

    /// <summary>
    /// Represents the treasure interacting with the stage. 
    /// The Update(Gametime) handles both user keyboard and gamepad controller input.
    /// If there is a gamepad attached the keyboard inputs are not processed.
    /// 
    /// removed game controller code from Update()
    /// 
    /// 2/8/2014 last changed
    /// </summary>

    public class Treasure : Model3D
    {
        private KeyboardState oldKeyboardState;
        private int rotate;
        private float angle;
        private Matrix initialOrientation;

        public Treasure(Stage theStage, string label, string meshFile)
            : base(theStage, label, meshFile)
        {
            IsCollidable = true;  // players test collision with Collidable set.
            rotate = 0;
            angle = 0.01f;
        }
    }
}
