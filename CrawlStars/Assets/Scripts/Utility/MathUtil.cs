using UnityEngine;

namespace Utility {
    public static class MathUtil {
        public static float GetAngle(Vector2 direction) {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
