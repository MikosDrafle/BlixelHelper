global using System;
global using Celeste.Mod.UI;
global using FMOD.Studio;
global using Microsoft.Xna.Framework;
global using Monocle;
global using Celeste;
global using System.Collections;
global using System.Diagnostics;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using Celeste.Mod.Entities;
global using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.BlixelHelper.Entities;
using Celeste.Mod.BlixelHelper.utils;
using Celeste.Mod.BlixelHelper.Entities.Solids;
using MonoMod.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Tracing;
// ReSharper disable All

namespace Celeste.Mod.BlixelHelper;

public class BlixelHelperModule : EverestModule {
    public static BlixelHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(BlixelHelperModuleSettings);
    public static BlixelHelperModuleSettings Settings => (BlixelHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(BlixelHelperModuleSession);
    public static BlixelHelperModuleSession Session => (BlixelHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(BlixelHelperModuleSaveData);
    public static BlixelHelperModuleSaveData SaveData => (BlixelHelperModuleSaveData) Instance._SaveData;

    public BlixelHelperModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(BlixelHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(BlixelHelperModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        //hook loading here
        On.Celeste.Player.Jump += JumpDirectionChange;
        On.Celeste.Player.SuperWallJump += PulseBounce;

        On.Celeste.Player.StarFlyUpdate += WaveFeather;

        On.Celeste.Player.StarFlyBegin += WaveFeatherBegin;
    }

    private void WaveFeatherBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self)
    {
        if (self.Scene.Tracker.CountEntities<WaveFlyController>() > 0)
        {
            float FeatherData = self.Facing == Facings.Left ? -1f : 1f;
            if (self.Scene.Tracker.CountEntities<WaveFlyController>() > 0)
            {
                DynamicData data = DynamicData.For(self);

                data.Set("WaveDirection", FeatherData);
                data.Set("WaveSfxPlayed", false);
            }
            self.Speed = new Vector2(170f * FeatherData, (Input.MenuUp.Check || Input.Jump.Check) ? -170f : 170f);
            self.starFlyTimer = 3f;
        }
        orig(self);
    }

    private int WaveFeather(On.Celeste.Player.orig_StarFlyUpdate orig, Player self)
    {
        if (self.StateMachine.State == Player.StStarFly && self.Scene.Tracker.CountEntities<WaveFlyController>()>0)
        {
            DynamicData data = DynamicData.For(self);
            var featherData = (float?)data.Get("WaveDirection");
            featherData ??= 1f;

            float speedX = 170f;

            if (Input.Grab.Check)
            {
                speedX = 320f;
            }
            float aim = Input.GetAimVector().Y;
            self.Speed = Calc.Approach(new Vector2((float)featherData * speedX, self.Speed.Y), new Vector2((float)featherData * 170f, (aim<0 || Input.Jump.Check) ? -170f : 170f), 4096f*Engine.DeltaTime);
            self.starFlyTimer -= Engine.DeltaTime;

            if (self.starFlyTimer <= Player.StarFlyEndFlashDuration && (bool)data.Get("WaveSfxPlayed")==false)
            {
                data.Set("WaveSfxPlayed", true);
                self.starFlyWarningSfx.Play(SFX.game_06_feather_state_warning);
            }

            if (self.starFlyTimer <= Player.StarFlyEndFlashDuration && self.Scene.OnInterval(0.05f))
            {
                if (self.Sprite.Color == self.starFlyColor)
                    self.Sprite.Color = Player.NormalHairColor;
                else
                    self.Sprite.Color = self.starFlyColor;
            }

            if (self.starFlyTimer<=0f || Input.Dash.Pressed)
            {
                return 0;
            }
        }

        return orig(self);
    }

    private void PulseBounce(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        orig(self, dir);
        foreach (DashPulseBlock block in self.Scene.Entities.OfType<DashPulseBlock>())
        {
            if ((block.Left <= self.Right+5 && block.Right >= self.Left-5) && (self.Top <= block.Bottom && self.Bottom >= block.Top) && block.wallBouncePulse)
            {
                block.Pulse(self);
                self.Speed += ((block.end - block.start).SafeNormalize() * (block.PulseStrength / 3f)) - (Vector2.UnitY*24f);
            }
        }
    }

    private void JumpDirectionChange(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        orig(self, particles, playSfx);
        Vector2 aimVector = Input.GetAimVector();

        if (self.Scene.Entities.OfType<JumpDirectionController>().Any())
        {
            if (MathF.Sign(self.Speed.X) != (MathF.Sign(aimVector.X)))
            {
                self.Speed.X *= -1;
            }
        }
    }

    public override void Unload() {
        //hook unloading here
        On.Celeste.Player.Jump -= JumpDirectionChange;
        On.Celeste.Player.SuperWallJump -= PulseBounce;

        On.Celeste.Player.StarFlyUpdate -= WaveFeather;

        On.Celeste.Player.StarFlyBegin -= WaveFeatherBegin;
    }


}