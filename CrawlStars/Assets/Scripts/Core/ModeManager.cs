using Cysharp.Threading.Tasks;

namespace Core {
    public class ModeManager {
        private static ModeManager instance;
        public static ModeManager Instance => instance ??= new ModeManager();
        
        public enum GameMode { Solo, Team }

        public GameMode CurGameMode { get; set; }
        public ModeInfo ModeInfo { get; private set; }

        public async UniTask Initialize() {
            // Load
        }
        
    }
}