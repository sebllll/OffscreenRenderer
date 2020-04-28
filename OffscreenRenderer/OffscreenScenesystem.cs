using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Input;
using Xenko.Engine;
using Xenko.Rendering;
using Xenko.Rendering.Lights;
using Xenko.Rendering.Colors;
using Xenko.Rendering.Compositing;

namespace OffscreenRenderer
{
    public class OffscreenScenesystem : SyncScript
    {

        public override void Start()
        {
            var scene = new Scene();

            AddSceneContent(scene);

            // Create SCenesystem
            var sceneSystem = new SceneSystem(Services)
            {
                SceneInstance = new SceneInstance(Services, scene)
            };
            sceneSystem.Name = "OffscreeScenesystem";

            // OffscreenCompositor
            var offscreencompositor = Content.Load<GraphicsCompositor>("OffscreenCompositor");
            offscreencompositor.Name = "OffscreenCompositor";

            sceneSystem.GraphicsCompositor = offscreencompositor;

            // add Camera
            var camEntity = new Entity();
            var camComponent = new CameraComponent();
            camEntity.Add(camComponent);
            scene.Entities.Add(camEntity);

            //Assign camera to CameraSlot
            camComponent.Slot = sceneSystem.GraphicsCompositor.Cameras.FirstOrDefault().ToSlotId();

            


            // add scenesystem to Game, so it gets called
            Game.GameSystems.Add(sceneSystem);
        }


        private void AddSceneContent(Scene scene)
        {
            // Create Capsule
            var capsule = new Entity();

            // Add a model included in the game files.
            var modelComponent = capsule.GetOrCreate<ModelComponent>();
            modelComponent.Model = Content.Load<Model>("Capsule");

            //Create Light
            var amblight = new Entity();

            var lightCol = new Color3(1.0f);

            var lc = new LightComponent
            {
                Type = new LightAmbient { Color = new ColorRgbProvider(lightCol) },
                Intensity = 500.0f
            };
            amblight.Add(lc);

            // add Entities to Scene
            scene.Entities.Add(capsule);
            scene.Entities.Add(amblight);
        }

        public override void Update()
        {
            var myscene = (SceneSystem)Game.GameSystems.Where(system => system.Name == "OffscreeScenesystem").FirstOrDefault();

            //myscene.Update(Game.UpdateTime);
            //myscene.Draw(Game.UpdateTime);

            //myscene.SceneInstance.Update(Game.UpdateTime);
            //myscene.Draw();

            //if (myscene.GraphicsCompositor.Cameras.FirstOrDefault().Camera == null)
            //{
            //    var camComponent = new CameraComponent();
            //    camComponent.Slot = myscene.GraphicsCompositor.Cameras.FirstOrDefault().ToSlotId();
            //}
        }
    }
}
