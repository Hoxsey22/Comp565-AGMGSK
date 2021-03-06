﻿/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Player.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
    MonoGames 3.2 to MonoGames 3.4  

    AGMGSKv7 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Cloud.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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
    /// An example of how to override the Model3D's Update(GameTime) to 
    /// animate a model's objects.  The actual update of values is done by calling 
    /// each instance object and setting its (Pitch, Yaw, Roll, or Step property. 
    /// Then call base.Update(GameTime) method of MovableModel3D to apply transformations.
    /// </summary>
    public class Treasure : Model3D
    {
        private Random random;
        private Vector3 scale = new Vector3(100);
        private int heightSpacing = 75;

        // Constructor
        public Treasure(Stage stage, string label, string meshFile, int nTreasure)
            : base(stage, label, meshFile)
        {
            random = new Random();
            addObject(new Vector3(447 * stage.Spacing, stage.Terrain.surfaceHeight(447, 453) + heightSpacing, 453 * stage.Spacing), Vector3.Up, 0.79f, scale);
            for (int i = 1; i < nTreasure; i++)
            {
                int x = (128 + random.Next(256)) * stage.Spacing;  // 128 .. 384
                int z = (128 + random.Next(256)) * stage.Spacing;
                addObject(
                    new Vector3(x, stage.surfaceHeight(x, z) + heightSpacing, z),
                    Vector3.Up, 1.57f,
                    scale);
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Object3D obj in instance)
            {
                base.Update(gameTime);
            }

        }
    }
}
