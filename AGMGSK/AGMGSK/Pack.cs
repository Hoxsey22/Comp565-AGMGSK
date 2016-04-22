/*  
    Copyright (C) 2016 G. Michael Barnes
 
    The file Pack.cs is part of AGMGSKv7 a port and update of AGXNASKv6 from
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

namespace AGMGSKv7 {

/// <summary>
/// Pack represents a "flock" of MovableObject3D's Object3Ds.
/// Usually the "player" is the leader and is set in the Stage's LoadContent().
/// With no leader, determine a "virtual leader" from the flock's members.
/// Model3D's inherited List<Object3D> instance holds all members of the pack.
/// 
/// 2/1/2016 last changed
/// </summary>
    public class Pack : MovableModel3D
    {
        Object3D leader;
        double[] probability = new double[] {
            0.0,
            0.33,
            0.66,
            0.99
        };
        int packingLevel = 0;
        int nDogs = 0;


        /// <summary>
        /// Construct a pack with an Object3D leader
        /// </summary>
        /// <param name="theStage"> the scene </param>
        /// <param name="label"> name of pack</param>
        /// <param name="meshFile"> model of a pack instance</param>
        /// <param name="xPos, zPos">  approximate position of the pack </param>
        /// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
        public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
            : base(theStage, label, meshFile)
        {
            isCollidable = true;
            random = new Random();
            leader = theLeader;
            int spacing = stage.Spacing;
            // initial vertex offset of dogs around (xPos, zPos)
            int[,] position = { { 0, 0 }, { 7, -4 }, { -5, -2 }, { -7, 4 }, { 5, 2 } };
            for (int i = 0; i < position.GetLength(0); i++)
            {
                int x = xPos + position[i, 0];
                int z = zPos + position[i, 1];
                float scale = (float)(0.5 + random.NextDouble());
                addObject(new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
                              new Vector3(0, 1, 0), 0.0f,
                              new Vector3(scale, scale, scale));
            }
            this.nDogs = nDogs;
        }

        public int NumberOfDogs
        {
            get { return nDogs; }
        }

        public Object3D Leader
        {
            get { return leader; }
            set { leader = value; }
        }
        public Double Probability
        {
            get { return probability[packingLevel % 4]; }
        }

        public int Level
        {
            get { return packingLevel % 4; }
            set { packingLevel = value; }
        }

        public void freeRoam(Object3D dog)
        {
            float angle = 0.3f;

            foreach (Object3D obj in instance)
            {
                obj.Yaw = 0.0f;
                // change direction 4 time a second  0.07 = 4/60
                if (random.NextDouble() < 0.07)
                {
                    if (random.NextDouble() < 0.5) dog.Yaw -= angle; // turn left
                    else dog.Yaw += angle; // turn right
                }
            }
        }

        public void packing(Object3D dog)
        {
            float angle = (float)(Math.PI / 180);
            dog.Yaw = 0.0f;

            Vector3 alignmentVector = getAlignmentVector(dog);
            Vector3 cohesionVector = getCohesionVector(dog);
            Vector3 separationVector = getSeparationVector(dog);
            Vector3 dogForward = dog.Forward;
            Vector3 flockingVector = alignmentVector + cohesionVector + separationVector;

            if (flockingVector == Vector3.Zero)
            {
                return;
            }

            flockingVector.Normalize();
            dogForward.Normalize();

            if (Vector3.Distance(dogForward, flockingVector) <= 0.01)
            {
                return;
            }
            if (Vector3.Distance(Vector3.Negate(dogForward), flockingVector) <= 0.01)
            {
                dogForward.X += 0.05f;
                dogForward.Z += 0.05f;
                dogForward.Normalize();
            }

            Vector3 AoR = Vector3.Cross(dogForward, flockingVector);
            AoR.Normalize();

            if (AoR.X + AoR.Y + AoR.Z < 0)
            {
                angle = -angle;
            }

            dog.Yaw += angle;

        }
        public Vector3 getSeparationVector(Object3D dog)
        {

            // init the variable needed
            Vector3 separationVector = Vector3.Zero;
            Vector3 otherDogPosition;
            Vector3 newVector;
            Vector3 dogPosition = new Vector3(dog.Translation.X, 0, dog.Translation.Z);
            Vector3 leaderPosition = new Vector3(leader.Translation.X, 0, leader.Translation.Z);

            // gets the distance between the passed dog and leader
            float distance = Vector3.Distance(dog.Translation, leader.Translation);
            float separationBoundingArea = 1000.0f;

            // checks if the distance between dog and leader is less than the bounding area of the separatial force
            if (distance < separationBoundingArea)
            {
                // Goes through all the dogs and adjusts new vector based on the position difference
                foreach (Object3D oDogs in instance)
                {
                    if (oDogs != dog)
                    {
                        otherDogPosition = new Vector3(oDogs.Translation.X, 0, oDogs.Translation.Z);
                        newVector = otherDogPosition - dogPosition;
                        separationVector = separationVector - newVector;
                    }
                }

                newVector = leaderPosition - dogPosition;
                separationVector = separationVector - newVector;
                return Vector3.Normalize(separationVector);
            }

            return Vector3.Zero;
        }
        public Vector3 getCohesionVector(Object3D dog)
        {

            // gets the distance between the passed dog and leader
            float distance = Vector3.Distance(dog.Translation, leader.Translation);
            float cohesionBoundingArea = 3000.0f;

            // checks distance is greater than the cohesion bounding area if so adjust cohesion vector
            if (distance > cohesionBoundingArea)
            {
                Vector3 dogPosition = new Vector3(dog.Translation.X, 0, dog.Translation.Z);
                Vector3 leaderPosition = new Vector3(leader.Translation.X, 0, leader.Translation.Z);
                Vector3 newVector = leaderPosition - dogPosition;

                return Vector3.Normalize(newVector);
            }
            return Vector3.Zero;

        }
        public Vector3 getAlignmentVector(Object3D dog)
        {
            float alignmentStart = 1000.0f;
            float alignmentEnd = 3000.0f;
            float distance = Vector3.Distance(dog.Translation, leader.Translation);

            Vector3 alignmentVector = new Vector3(leader.Forward.X, 0, leader.Forward.Z);

            if ((distance > alignmentStart) && (distance < alignmentEnd))
            {
                return Vector3.Normalize(alignmentVector);
            }
            return Vector3.Zero;
        }


        /// <summary>
        /// Each pack member's orientation matrix will be updated.
        /// Distribution has pack of dogs moving randomly.  
        /// Supports leaderless and leader based "flocking" 
        /// </summary>      
        public override void Update(GameTime gameTime)
        {
            foreach (Object3D dog in instance)
            {
                if (random.NextDouble() < probability[packingLevel % 4])
                    packing(dog);
                else
                    freeRoam(dog);

                dog.updateMovableObject();

                stage.setSurfaceHeight(dog);
            }
            base.Update(gameTime);  // MovableMesh's Update(); 
        }
    }
}
