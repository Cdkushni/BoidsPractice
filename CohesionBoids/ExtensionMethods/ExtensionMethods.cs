using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CohesionBoids.ExtensionMethods
{
    public static class Extensions
    {
        public static void Average(this Vector2 vector, float count)
        {
            vector.X /= count;
            vector.Y /= count;
        }

        public static Vector2 Origin(this Texture2D texture)
        {
            return new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }
    }
}
