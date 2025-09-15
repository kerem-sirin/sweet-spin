# Sweet Spin Slots v1.0

A Unity-based 5x3 slot machine game with 25 paylines, featuring a candy/fruit theme and comprehensive game mechanics.

## ● Play Now
[Play on itch.io](https://bluebuckgames.itch.io/sweet-spin)

## ● Screenshots
<figure>
<img width="400" height="600" alt="Sweet Spin Gameplay" src="https://github.com/user-attachments/assets/9a8933d3-0064-4fb9-8e82-7de5e1a03193" />
  <figcaption>Sweet Spin slot machine gameplay showing the 5x3 reel layout with candy/fruit symbols and 25 paylines.</figcaption>
</figure>

 ## ● Highlights
 - Verified 94.46% RTP through 1M+ spin simulation
 - Clean MVC architecture with event-driven design
 - Comprehensive simulation tool included
 - Full source code available

## ● Features
- **10 unique symbols** with candy/fruit theme
- **25 paylines** for multiple winning combinations
- **Turbo Mode** for faster gameplay
- **Auto-play** functionality (3, 7, or 15 spins)
- **Win animations** with particle effects
- **Comprehensive audio system** with music ducking
- **Responsive UI** with bet controls

## ● Game Statistics
- **RTP (Return to Player):** 94.46%
- **Hit Frequency:** 36.79% (1 in 2.72 spins)
- **Volatility:** Medium-Low
- **Max Win:** 1650x line bet
- **Paylines:** 25

### Verified Simulation Results
- [1 Million Spins - Summary](Assets/SimulationResults/20250915_024733.json) - Statistical analysis
- [1,000 Spins - Detailed](Assets/SimulationResults/20250915_024636.json) - Including turn-by-turn data

<figure>
<img width="300" height="750" alt="Simulation Tool Interface" src="https://github.com/user-attachments/assets/12cdc33c-440c-445b-bee0-8b7039b9a5e4" />
  <figcaption>Built-in simulation tool interface for RTP verification and mathematical testing with configurable parameters.</figcaption>
</figure>

## ● Technical Architecture
- **Design Pattern:** MVC with Service Locator
- **Event System:** Custom event bus for decoupled communication
- **State Machine:** For game flow management
- **Simulation System:** Built-in RTP verification tool
- **Save System:** PlayerPrefs-based persistence

## ● Project Structure
```plaintext
Assets/Scripts/
├── Core/          # Game logic and controllers
├── Model/         # Data structures
├── View/          # UI components
├── Service/       # Game services
├── Events/        # Event definitions
└── Simulation/    # RTP testing system
```

## ● Built With
- Unity 6000.0.49f1
- C# 
- DOTween for animations
- TextMeshPro for UI

## ● Simulation System
The project includes a professional-grade simulation system that verifies game mathematics without requiring actual gameplay.

### Features:
- **Headless Execution**: Runs game logic without rendering, achieving 1M+ spins in seconds
- **Statistical Analysis**: Tracks hit frequency, RTP, symbol distribution, and win patterns
- **Batch Processing**: Run multiple simulations consecutively for variance analysis
- **JSON Export**: Outputs detailed reports for external analysis
- **Configurable Parameters**: Test different bet sizes, starting credits, and spin counts

### Key Metrics Tracked:
- Return to Player (RTP) percentage
- Hit frequency and distribution
- Symbol win frequency
- Payline hit patterns
- Win tier distribution (Small/Medium/Big/Mega/Jackpot)
- Session progression over time

### How to Use:
1. Open Unity Editor
2. Navigate to `Tools > Sweet Spin > Simulation Runner`
3. Configure parameters:
   - Starting Credits
   - Bet Per Line
   - Maximum Turns
   - Save Turn Details (disable for large simulations)
4. Click "Run Simulation"
5. Results are saved to `Assets/SimulationResults/`

This simulation system ensures mathematical fairness and helps balance game economy without manual testing.

## ● Getting Started
1. Clone the repository
2. Open in Unity 6000.0.49f1 or later
3. Open the `Assets/Scenes/Main.unity` scene
4. Press Play to test in editor

### Building for WebGL
1. File > Build Settings
2. Select WebGL platform
3. Player Settings > Publishing Settings > Compression Format: Gzip
4. Build and deploy to web server

## ● Development Notes
- Payline patterns are defined in JSON for easy modification
- Symbol payouts are configurable via ScriptableObject
- All animations use DOTween for smooth performance
- Event bus pattern ensures loose coupling between systems

## ● Author
Kerem Sirin - 09/2025

## ● License
- This project is available for portfolio demonstration purposes.
- MIT License - See LICENSE file for details
