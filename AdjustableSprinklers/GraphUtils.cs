using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AdjustableSprinklers
{
    public class GraphUtils
    {
        public static IEnumerable<Vector2> GetNeighbors(IEnumerable<Vector2> allVectors, Vector2 vector)
        {
            return new[]
            {
                new Vector2(1, 0) + vector,
                new Vector2(0, 1) + vector,
                new Vector2(-1, 0) + vector,
                new Vector2(0, -1) + vector,
            }.Where(allVectors.Contains);
        }

        public static IEnumerable<Vector2> GetDisconnected(IEnumerable<Vector2> allVectors, Vector2 vector)
        {
            var allVectorsArray = allVectors as Vector2[] ?? allVectors.ToArray();
            
            var visited = new HashSet<Vector2>();
            var stack = new Stack<Vector2>();

            stack.Push(vector);
            visited.Add(vector);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                foreach (var neighbor in GetNeighbors(allVectorsArray, current)
                    .Where(neighbor => !visited.Contains(neighbor)))
                {
                    stack.Push(neighbor);
                    visited.Add(neighbor);
                }
            }

            return allVectorsArray.Where(x => !visited.Contains(x));
        }
    }
}