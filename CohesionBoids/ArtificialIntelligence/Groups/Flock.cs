using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CohesionBoids.ArtificialIntelligence.Groups
{
    public class Flock
    {
        public static Random r = new Random();
        public Entity Leader { get; private set; }
        public List<Entity> Entities { get; private set; }
        public Color RandomColor { get; private set; }

        public Flock(Entity leader, GameTime gameTime)
        {
            Leader = leader;

            Entities = new List<Entity>();
            Entities.Add(leader);

            RandomColor = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
        }

        public void AddMember(Entity e)
        {
            Entities.Add(e);
        }

        public void RemoveFarEntitiesFromLeader()
        {
            foreach (Entity e in Entities.Where(x => x != Leader && Vector2.DistanceSquared(x.Position, Leader.Position) > Leader.DetectionDistance).ToList())
            {
                Entities.Remove(e);
                e.RemoveFromFlock();
            }
        }
    }
}
