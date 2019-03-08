using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using CohesionBoids.ArtificialIntelligence;
using CohesionBoids.ArtificialIntelligence.Groups;
using CohesionBoids.ExtensionMethods;

namespace CohesionBoids
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private List<Entity> entityList;
        private const float EntitySpeed = 150;
        private const float MaxEntitySpeed = 200;
        private const float EntityScale = 0.25f;
        private const float EntityDetectionDistance = 150;
        private const float EntitySeparationDistance = 50;
        private const int BoundingBoxExpansion = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            //Console.WriteLine(Content.RootDirectory);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            entityList = new List<Entity>();

            Rectangle rectangle = new Rectangle(-BoundingBoxExpansion, -BoundingBoxExpansion, GraphicsDevice.Viewport.Width + BoundingBoxExpansion, GraphicsDevice.Viewport.Height + BoundingBoxExpansion);
            float rotation = MathHelper.ToRadians(-10);

            Entity e13 = new Entity(rectangle, new Vector2(20, 60), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e46 = new Entity(rectangle, new Vector2(80, 120), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e37 = new Entity(rectangle, new Vector2(60, 140), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e49 = new Entity(rectangle, new Vector2(80, 180), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e22 = new Entity(rectangle, new Vector2(100, 500), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e23 = new Entity(rectangle, new Vector2(500, 500), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);
            Entity e24 = new Entity(rectangle, new Vector2(200, 200), EntitySpeed, MaxEntitySpeed, rotation, EntityScale, EntityDetectionDistance, EntitySeparationDistance);

            //Avoid finding new neighbors at runtime, so we are generating the flock ahead of time
            Flock f = new Flock(e13, null);
            e13.Flock = f;
            e13.InFlock = true;

            f.AddMember(e46);
            e46.Flock = f;
            e46.InFlock = true;

            f.AddMember(e37);
            e37.Flock = f;
            e37.InFlock = true;

            f.AddMember(e49);
            e49.Flock = f;
            e49.InFlock = true;

            f.AddMember(e22);
            e22.Flock = f;
            e22.IsFlockLeader = true;
            e22.InFlock = true;

            f.AddMember(e23);
            e23.Flock = f;
            e23.InFlock = true;

            f.AddMember(e24);
            e24.Flock = f;
            e24.InFlock = true;


            entityList.Add(e13);
            entityList.Add(e46);
            entityList.Add(e37);
            entityList.Add(e49);
            entityList.Add(e22);
            entityList.Add(e23);
            entityList.Add(e24);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Texture2D entityTexture = Content.Load<Texture2D>("entity");
            Texture2D entityLeaderTexture = Content.Load<Texture2D>("entity-leader");
            Texture2D areaTexture = Content.Load<Texture2D>("area");

            entityList.ForEach(x => x.AssignTextures(entityTexture, entityLeaderTexture, areaTexture));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            foreach (Entity e in entityList)
            {
                if (gameTime.TotalGameTime.TotalSeconds > 1)
                    e.Velocity += Cohesion(e);
                e.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public Vector2 Cohesion(Entity Entity)
        {
            Vector2 forceVector = Vector2.Zero;
            Vector2 centerOfMass = Vector2.Zero;
            int neighbors = 0;

            foreach (Entity e in Entity.Flock.Entities)
            {
                //Check to make sure the neighbor we are investigating is not ourself
                if (e != Entity)
                {
                    centerOfMass += e.Position;
                    neighbors++;
                }
            }

            if (neighbors > 0)
            {
                centerOfMass.Average(neighbors);

                forceVector = centerOfMass - Entity.Position;

                forceVector = Vector2.Normalize(forceVector);
            }

            return forceVector;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            entityList.ForEach(x => x.Draw(gameTime, spriteBatch));
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
