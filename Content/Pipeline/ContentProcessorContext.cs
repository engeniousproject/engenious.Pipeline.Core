using System;
using System.Threading;

namespace engenious.Content.Pipeline
{
    public class ContentProcessorContext : ContentContext
    {
        static ContentProcessorContext()
        {
            
            //BaseWindow = new GameWindow();
            //_window = new NativeWindow(100,100,"Test",GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);

        }

        public ContentProcessorContext(SynchronizationContext syncContext, CreatedContentCode createdContentCode, object game, Guid buildId, string contentDirectory, string workingDirectory = "")
            : base(buildId, createdContentCode, contentDirectory, workingDirectory)
        {
            SyncContext = syncContext;


            Game = game;
            // GraphicsDevice.SwitchUiThread();
        }

        public SynchronizationContext SyncContext { get; }
        public object Game { get; }
        // public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
        
        public override void Dispose()
        {
            // GraphicsDevice.Context.MakeNoneCurrent();
            //Window.Dispose();
        }
    }
}