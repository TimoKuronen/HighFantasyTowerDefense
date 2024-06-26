public class GameServices : Services
{
    protected override void Initialize()
    {
        var inputManager = new InputManager();
        AddService<IInputManager>(inputManager);

        var gameManager = new GameManager(inputManager);
        AddService<IGameManager>(gameManager);

        var pathFinder = new Pathfinder();
        AddService<IPathFinder>(pathFinder);

        var economicsManager = new EconomicManager();
        AddService<IEconomicsManager>(economicsManager);

        var gemManager = new GemManager();
        AddService<IGemManager>(gemManager);

        var gameStateHandler = new GameStateHandler();
        AddService<IGameStateHandler>(gameStateHandler);

        var timeManager = new TimeManager();
        AddService<ITimeManager>(timeManager);
    }
}
