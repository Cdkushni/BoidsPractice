using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CohesionBoids.ArtificialIntelligence.Groups;
using CohesionBoids.ExtensionMethods;

namespace CohesionBoids.ArtificialIntelligence
{
    public class Entity
    {
        #region Graphics
        private Texture2D texture;

        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                if (value != null)
                {
                    texture = value;
                    TextureData = new Color[value.Width * value.Height];
                    Texture.GetData(TextureData);


                }
            }
        }

        private Texture2D leaderTexture;

        public Texture2D LeaderTexture
        {
            get { return leaderTexture; }
            set
            {
                if (value != null)
                {
                    leaderTexture = value;
                    LeaderTextureData = new Color[value.Width * value.Height];
                    LeaderTexture.GetData(LeaderTextureData);
                }
            }
        }

        public Color[] TextureData { get; private set; }
        public Color[] LeaderTextureData { get; private set; }

        public Texture2D AreaTexture { get; private set; }
        #endregion

        #region Physics
        public Rectangle BoundingRectangle { get; private set; }

        public Matrix Transform { get; private set; }

        public Rectangle Rectangle { get; private set; }

        private Vector2 position;
        public Vector2 Position { get { return this.position; } protected set { this.position = value; } }

        public float Speed { get; private set; }
        public float MaxSpeed { get; private set; }

        public Vector2 Velocity { get; set; }

        public float Rotation { get; private set; }

        public float Scale { get; private set; }
        public float DetectionAreaScale { get; private set; }
        public float SeparationAreaScale { get; private set; }
        #endregion

        #region Artificial Intelligence
        public float DetectionDistance { get; private set; }

        public float SeparationDistance { get; set; }

        public bool InFlock { get; set; }
        public bool IsFlockLeader { get; set; }
        public Flock Flock { get; set; }
        #endregion

        public Entity(Rectangle bounds, Vector2 position, float speed, float maxSpeed, float rotationRadians, float scale, float detectionDistance, float separationDistance)
        {
            BoundingRectangle = bounds;

            Position = position;

            Speed = speed;
            MaxSpeed = maxSpeed;
            Rotation = rotationRadians;

            //Velocity = new Vector2(speed * (float)Math.Cos(Rotation), speed * (float)Math.Sin(Rotation));
            Velocity = Vector2.Zero;

            Scale = scale;

            DetectionDistance = detectionDistance;

            SeparationDistance = separationDistance;
        }

        public void AssignTextures(Texture2D texture, Texture2D leaderTexture, Texture2D areaTexture)
        {
            Texture = texture;
            LeaderTexture = leaderTexture;
            AreaTexture = areaTexture;

            DetectionAreaScale = DetectionDistance / (float)areaTexture.Width;
            SeparationAreaScale = SeparationDistance / (float)areaTexture.Width;

            DetectionDistance = (float)Math.Pow(DetectionDistance, 2);
            SeparationDistance = (float)Math.Pow(SeparationDistance, 2);
        }

        /// <summary>
        /// Calculates the transform matrix of the object with the origin,
        /// rotation, scale, and position.  This will need to be done every
        /// game loop because chances are the position changed.
        /// </summary>
        private void CalculateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(-Texture.Origin(), 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Scale) *
                Matrix.CreateTranslation(new Vector3(Position, 0));
        }

        /// <summary>
        /// Calculates the bounding rectangle of the object using the object's transform
        /// matrix to make a correct rectangle.
        /// </summary>
        private void CalculateBoundingRectangle()
        {
            if (Texture != null)
            {
                Rectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
                Vector2 leftTop = Vector2.Transform(new Vector2(Rectangle.Left, Rectangle.Top), Transform);
                Vector2 rightTop = Vector2.Transform(new Vector2(Rectangle.Right, Rectangle.Top), Transform);
                Vector2 leftBottom = Vector2.Transform(new Vector2(Rectangle.Left, Rectangle.Bottom), Transform);
                Vector2 rightBottom = Vector2.Transform(new Vector2(Rectangle.Right, Rectangle.Bottom), Transform);

                Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                  Vector2.Min(leftBottom, rightBottom));
                Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                          Vector2.Max(leftBottom, rightBottom));

                Rectangle = new Rectangle((int)min.X, (int)min.Y,
                    (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Velocity != Vector2.Zero)
            {
                Velocity = Vector2.Normalize(Velocity);
                Velocity = Vector2.Multiply(Velocity, Speed);

                Rotation = (float)Math.Atan2((double)Velocity.Y, (double)Velocity.X);
                Position += Vector2.Multiply(Velocity, (float)gameTime.ElapsedGameTime.TotalSeconds);

                if (Rectangle.Right < BoundingRectangle.Left || Rectangle.Left > BoundingRectangle.Right || Rectangle.Bottom < BoundingRectangle.Top || Rectangle.Top > BoundingRectangle.Bottom)
                {
                    MoveInsideBoundingRectangle();
                }

                CalculateMatrix();
                CalculateBoundingRectangle();
            }
        }

        public void MoveInsideBoundingRectangle()
        {
            if (Rectangle.Right < BoundingRectangle.Left)
                position.X = BoundingRectangle.Right;
            else if (Rectangle.Left > BoundingRectangle.Right)
                position.X = BoundingRectangle.Left;

            if (Rectangle.Bottom < BoundingRectangle.Top)
                position.Y = BoundingRectangle.Bottom;
            else if (Rectangle.Top > BoundingRectangle.Bottom)
                position.Y = BoundingRectangle.Top;
        }

        public void RefreshNeighborList(IEnumerable<Entity> allEntities, GameTime gameTime)
        {
            FindNewNeighbors(allEntities, gameTime);
        }

        private void FindNewNeighbors(IEnumerable<Entity> allEntities, GameTime gameTime)
        {
            foreach (Entity e in allEntities.Where(e => e != this).Where(e => !e.InFlock).Where(e => Vector2.DistanceSquared(e.Position, Position) < DetectionDistance))
            {
                if (Flock == null)
                {
                    Flock = new Flock(this, gameTime);
                    InFlock = true;
                    IsFlockLeader = true;
                }

                if (!Flock.Entities.Contains(e))
                {
                    Flock.AddMember(e);
                    e.Flock = Flock;
                    e.InFlock = true;
                }
            }
        }

        public void RemoveFromFlock()
        {
            Flock = null;
            InFlock = false;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Color c = Flock != null ? Flock.RandomColor : Color.Black * 0.25f;

            /*if (AreaTexture != null)
            {
                spriteBatch.Draw(AreaTexture, Position, null, Color.Red * 0.25f, 0, AreaTexture.Origin(), DetectionAreaScale, SpriteEffects.None, 0);
                spriteBatch.Draw(AreaTexture, Position, null, Color.Yellow * 0.5f, 0, AreaTexture.Origin(), SeparationAreaScale, SpriteEffects.None, 0);
            }*/

            if (Texture != null)
            {
                spriteBatch.Draw(InFlock && IsFlockLeader ? LeaderTexture : Texture, Position, null, c, Rotation, Texture.Origin(), Scale, SpriteEffects.None, 0);
            }
        }
    }
}
