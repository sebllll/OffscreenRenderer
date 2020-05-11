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
using Xenko.Rendering.Skyboxes;
using Xenko.Rendering.Colors;
using Xenko.Rendering.Compositing;
using Xenko.Graphics;

namespace OffscreenRenderer
{
    public class OffscreenScenesystem : SyncScript
    {
        public override void Start()
        {
            var scene = new Scene();

            AddSceneContent(scene);

            // add Camera
            var camEntity = new Entity();
            var camComponent = new CameraComponent();
            camEntity.Add(camComponent);
            scene.Entities.Add(camEntity);

            // transform cam pos
            camEntity.Transform.Position = new Vector3(0.0f, 0.0f, 5.0f);
            var angle = (float)Math.PI / -2;
            camEntity.Transform.Rotation = Quaternion.RotationZ(angle);


            // Create Scenesystem
            var sceneSystem = new SceneSystem(Services)
            {
                SceneInstance = new SceneInstance(Services, scene)
            };
            sceneSystem.Name = "OffscreeScenesystem";

            var renderTargetloaded = Content.Load<Texture>("RenderTexture");
            //var renderTarget = Helpers.CreateRenderTarget(Game.GraphicsDevice, PixelFormat.R8G8B8A8_UNorm_SRgb, 1024, 1024); // works also, but this is not set in GS's Material
            var offscreencompositorFromCode = Helpers.CreateOffscreenCompositor(false, renderTargetloaded, camComponent);

            sceneSystem.GraphicsCompositor = offscreencompositorFromCode;

            // add scenesystem to Game, so it gets called
            Game.GameSystems.Add(sceneSystem);
        }

        private Entity dirlight;

        private void AddSceneContent(Scene scene)
        {
            // Capsule
            var capsule = new Entity();
            var modelComponent = capsule.GetOrCreate<ModelComponent>();
            modelComponent.Model = Content.Load<Model>("Capsule");

            // Ambient Light
            var amblight = new Entity();

            var lightCol = new Color3(1.0f, 0.0f, 0.0f);

            var lc = new LightComponent
            {
                Type = new LightAmbient { Color = new ColorRgbProvider(lightCol) },
                Intensity = 0.2f
            };
            amblight.Add(lc);

            // Directional Light
            dirlight = new Entity();

            var dlc = new LightComponent
            {
                Type = new LightDirectional { Color = new ColorRgbProvider(new Color3(09.7f))},
                Intensity = 0.01f
            };
            dirlight.Add(dlc);

            // Sky Light 
            var skylight = new Entity();
            var skybox = Content.Load<Skybox>("Skybox");
            var slc = new LightComponent
            {
                Type = new LightSkybox { Skybox = skybox},
                Intensity = 0.02f
            };

            // Background
            ////var bgTexture = Content.Load<Texture>("Skybox texture");
            ////var bgc = new BackgroundComponent
            ////{
            ////    Texture = bgTexture,
            ////    Intensity = 1.0f
            ////};
            //skylight.Add(bgc);


            skylight.Add(slc);


            // add Entities to Scene
            scene.Entities.Add(capsule);
            scene.Entities.Add(amblight);
            scene.Entities.Add(dirlight);
            scene.Entities.Add(skylight);
        }

        public override void Update()
        {
            var deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            dirlight.Transform.Rotation *= Quaternion.RotationYawPitchRoll(0.8f * deltaTime, 0.3f * deltaTime, 0.6f * deltaTime);

        }
    }
}
