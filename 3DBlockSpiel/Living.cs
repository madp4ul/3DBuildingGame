using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlockGame3D
{
    abstract class Living:Body
    {
        public float HP { get; private set; }
        public float Defense {get; protected set;}
        public float Damage { get; protected set; }

        public bool Peaceful { get; private set; }

        public bool IsDead { get; private set; }
        public DateTime TimeDied { get; private set; }

        public Living(GraphicsDevice device, Vector3 position,Vector3 lookDirection,
            float acceleration, float brakeAcceleration, float gravityAcceleration, float jumpAcceleration,
            float maxFallSpeed, float maxMovementSpeed, float movementStrength,
            float eyeHeight, float bodyHeight, float bodyWidth, float fieldOfView, float viewDistance)
            : base(device, position,lookDirection,
                acceleration, brakeAcceleration, gravityAcceleration, jumpAcceleration,
                maxFallSpeed, maxMovementSpeed, movementStrength, eyeHeight, bodyHeight, bodyWidth, fieldOfView, viewDistance)
        {
            this.Peaceful = false;

            this.Defense = 1;
            this.Damage = 5;
            this.HP = 10;
        }

        public void GetDamaged(float attackDamage)
        {
            this.HP -= (attackDamage-this.Defense);
            if (this.HP < 0)
            {
                this.TimeDied = DateTime.Now;
                this.IsDead = true;
            }
        }


    }
}
