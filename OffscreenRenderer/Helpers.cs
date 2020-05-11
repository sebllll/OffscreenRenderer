using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Rendering;
using Xenko.Rendering.Background;
using Xenko.Rendering.Images;
using Xenko.Rendering.Lights;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Shadows;
using Xenko.Rendering.Sprites;
using Xenko.Rendering.Compositing;
using Xenko.Particles.Rendering;
using Xenko.Rendering.UI;
using Xenko.SpriteStudio.Runtime;
using System.Runtime.CompilerServices;

namespace OffscreenRenderer
{
    public static class Helpers
    {

        /// <summary>
        /// Creates a graphics compositor programatically that renders into a Rendertarget. It can render meshes, sprites and backgrounds.
        /// </summary>
        public static GraphicsCompositor CreateOffscreenCompositor(
            bool enablePostEffects,
            Texture renderTarget,
            //string modelEffectName = "XenkoForwardShadingEffect",
            CameraComponent camera = null,
            //GraphicsProfile graphicsProfile = GraphicsProfile.Level_10_0,
            RenderGroupMask groupMask = RenderGroupMask.All)
        {
            #region Render stages
            var opaqueRenderStage = new RenderStage("Opaque", "Main") { SortMode = new StateChangeSortMode() };
            var transparentRenderStage = new RenderStage("Transparent", "Main") { SortMode = new BackToFrontSortMode() };
            var shadowMapCaster = new RenderStage("ShadowMapCaster", "ShadowMapCaster") { SortMode = new FrontToBackSortMode() };
            var shadowMapCasterrParaboloidRenderStage = new RenderStage("ShadowMapCasterParaboloid", "ShadowMapCasterParaboloid") { SortMode = new FrontToBackSortMode() };
            var shadowMapCasterCubeMapRenderStage = new RenderStage("ShadowMapCasterCubeMap", "ShadowMapCasterCubeMap") { SortMode = new FrontToBackSortMode() };
            var gBuffer = new RenderStage("GBuffer", "GBuffer") { SortMode = new FrontToBackSortMode() };
            #endregion

            #region RenderFeatures
            var meshRenderFeature = new MeshRenderFeature
            {
                PipelineProcessors =
                    {
                        new MeshPipelineProcessor() { TransparentRenderStage = transparentRenderStage },
                        new ShadowMeshPipelineProcessor() { DepthClipping = false, ShadowMapRenderStage = shadowMapCaster},
                        new ShadowMeshPipelineProcessor() { DepthClipping = true, ShadowMapRenderStage = shadowMapCasterrParaboloidRenderStage },
                        new ShadowMeshPipelineProcessor() { DepthClipping = true, ShadowMapRenderStage = shadowMapCasterCubeMapRenderStage }
                    },
                RenderFeatures =
                    {
                        new TransformRenderFeature(),
                        new SkinningRenderFeature(),
                        new MaterialRenderFeature(),
                        new ShadowCasterRenderFeature(),
                        new ForwardLightingRenderFeature()
                        {
                            LightRenderers =
                            {
                                new LightAmbientRenderer(),
                                new LightDirectionalGroupRenderer(),
                                new LightSkyboxRenderer(),
                                new LightClusteredPointSpotGroupRenderer(),
                                new LightPointGroupRenderer()
                            }
                        }
                    },
                RenderStageSelectors =
                    {
                        new MeshTransparentRenderStageSelector()
                        {
                            EffectName = "XenkoForwardShadingEffect",
                            OpaqueRenderStage = opaqueRenderStage,
                            TransparentRenderStage = transparentRenderStage,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "XenkoForwardShadingEffect.ShadowMapCaster",
                            ShadowMapRenderStage = shadowMapCaster,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "XenkoForwardShadingEffect.ShadowMapCasterParaboloid",
                            ShadowMapRenderStage = shadowMapCasterrParaboloidRenderStage,
                            RenderGroup = groupMask
                        },
                        new ShadowMapRenderStageSelector()
                        {
                            EffectName = "XenkoForwardShadingEffect.ShadowMapCasterCubeMap",
                            ShadowMapRenderStage = shadowMapCasterCubeMapRenderStage,
                            RenderGroup = groupMask
                        },
                        new MeshTransparentRenderStageSelector()
                        {
                            EffectName = "XenkoForwardShadingEffect.ShadowMapCaster",
                            OpaqueRenderStage = gBuffer,
                            RenderGroup = groupMask
                        }
                    }

            };

            var spriteRenderFeature = new SpriteRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SpriteTransparentRenderStageSelector()
                    {
                        EffectName = "Test", // TODO: Check this
                        OpaqueRenderStage = opaqueRenderStage,
                        TransparentRenderStage = transparentRenderStage
                    }
                }
            };

            var backgroundRenderFeature = new BackgroundRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SimpleGroupToRenderStageSelector()
                    {
                        EffectName = "Test",
                        RenderStage = opaqueRenderStage
                    }
                }
            };

            var uiRenderFeature = new UIRenderFeature()
            {
                RenderStageSelectors =
                {
                    new SimpleGroupToRenderStageSelector()
                    {
                        EffectName = "Test",
                        RenderStage = transparentRenderStage
                    }
                }
            };

            var particleEmitterRenderFeature = new ParticleEmitterRenderFeature()
            {
                RenderStageSelectors =
                {
                    new ParticleEmitterTransparentRenderStageSelector()
                    {
                        OpaqueRenderStage = opaqueRenderStage,
                        TransparentRenderStage = transparentRenderStage
                    }
                }
            };

            //TODO: add that when in VL Context (needs VL.Xenko nuget)
            //var vlLayerRenderfeature = new LayerRenderFeature()
            //{
            //    RenderStageSelectors =
            //    {
            //        new SimpleGroupToRenderStageSelector()
            //        {
            //            RenderStage = opaqueRenderStage,
            //            RenderGroup = groupMask
            //        }
            //    }

            //};
            #endregion

            #region Camera slots
            var offscreenCameraSlot = new SceneCameraSlot();
            if (camera != null)
                camera.Slot = offscreenCameraSlot.ToSlotId(); //TODO: hand over camera to constructor and use that one here?
            #endregion

            #region post fx
            var postProcessingEffects = enablePostEffects
                ? new PostProcessingEffects
                {
                    ColorTransforms =
                    {
                        Transforms =
                        {
                            new ToneMap(),
                        },
                    },
                }
                : null;

            if (postProcessingEffects != null)
            {
                postProcessingEffects.DisableAll();
                postProcessingEffects.ColorTransforms.Enabled = true;
            }
            #endregion

            #region Renderers
            var forwardRenderer = new ForwardRenderer
            {
                Clear = { ClearFlags = ClearRendererFlags.ColorAndDepth, Color = new Color4(0, 0, 0, 0) },
                GBufferRenderStage = gBuffer,
                LightProbes = true,
                MSAALevel = MultisampleCount.None,
                //MSAAResolver = new MSAAResolver() { FilterType = MSAAResolver.FilterTypes.BSpline, FilterRadius = 1.0f },
                OpaqueRenderStage = opaqueRenderStage,
                ShadowMapRenderStages = { shadowMapCaster },
                //SubsurfaceScatteringBlurEffect,
                TransparentRenderStage = transparentRenderStage,
                // TODO: add postFX once their alpha is sorted out
                PostEffects = postProcessingEffects
            };

            var singleViewforwardRenderer = new ForwardRenderer
            {
                Clear = { ClearFlags = ClearRendererFlags.ColorAndDepth, Color = new Color4(0, 0, 0, 0) },
                GBufferRenderStage = gBuffer,
                LightProbes = true,
                MSAALevel = MultisampleCount.None,
                //MSAAResolver = new MSAAResolver() { FilterType = MSAAResolver.FilterTypes.BSpline, FilterRadius = 1.0f },
                OpaqueRenderStage = opaqueRenderStage,
                ShadowMapRenderStages = { shadowMapCaster },
                //SubsurfaceScatteringBlurEffect,
                TransparentRenderStage = transparentRenderStage
            };
            #endregion

            #region Game
            var game = new SceneCameraRenderer()
            {
                Camera = offscreenCameraSlot,
                Child = new RenderTextureSceneRenderer()
                {
                    RenderTexture = renderTarget,
                    Child = forwardRenderer,
                }
            };
            #endregion

            return new GraphicsCompositor
            {
                Cameras = { offscreenCameraSlot },

                RenderStages =
                {
                    opaqueRenderStage,
                    transparentRenderStage,
                    shadowMapCaster,
                    shadowMapCasterrParaboloidRenderStage,
                    shadowMapCasterCubeMapRenderStage,
                    gBuffer
                },
                RenderFeatures =
                {
                    meshRenderFeature,
                    spriteRenderFeature,
                    backgroundRenderFeature,
                    uiRenderFeature,
                    particleEmitterRenderFeature,
                    //vlLayerRenderfeature
                },

                Game = game
            };

        }


        public static Texture CreateRenderTarget(GraphicsDevice device, PixelFormat format, int width, int height)
        {
            return Texture.New2D(device, width, height, format, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
        }
        public static Texture CreateRenderTargetStaging(GraphicsDevice device, PixelFormat format, int width, int height)
        {
            return Texture.New2D(device, width, height, format, TextureFlags.RenderTarget | TextureFlags.ShaderResource).ToStaging();
        }
    }
}
