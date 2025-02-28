using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Particles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Sets.GraniteSet.GraniteArmor.Projectiles
{
	public class EnergyStomp : ModProjectile
	{
		private bool Landed { get => Projectile.ai[0] != 0; set => Projectile.ai[0] = value ? 1 : 0; }

		public override string Texture => SpiritMod.EMPTY_TEXTURE;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Energy Stomp");

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.aiStyle = 0;
			Projectile.scale = 1f;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.friendly = true;
			Projectile.alpha = 255;
		}

		public override void OnSpawn(IEntitySource source) => SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce, Projectile.Center);

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			Projectile.Center = player.Center + new Vector2(0, 18);

			if (player.velocity.Y == 0f && !Landed) //Explode
			{
				float fallDistMax = 16;
				float fallDistance = MathHelper.Clamp((float)(player.position.Y / 16f) - player.fallStart, 0, fallDistMax) / fallDistMax;
				player.GetModPlayer<MyPlayer>().stompCooldown = 30;

				for (int j = 0; j < 12; j++)
				{
					Vector2 velocity = new Vector2(0f, -(fallDistance * 16) * Main.rand.NextFloat(0.2f, 1.5f));
					Vector2 position = player.position + new Vector2(player.width * Main.rand.NextFloat(0.0f, 1.0f));

					Dust killDust = Dust.NewDustPerfect(position, DustID.Electric, velocity, 0, default, 0.8f);
					killDust.noGravity = true;

					if (j < 6)
					{
						killDust = Dust.NewDustPerfect(player.Center + new Vector2(player.width * Main.rand.NextFloat(0.0f, 1.0f), player.height / 2 * player.gravDir), DustID.Electric, new Vector2((fallDistance * 16) * Main.rand.NextFromList(-1.0f, 1.0f), 0f), 0, default, 0.8f);
						killDust.noGravity = true;
						killDust.fadeIn = 1.2f;
					}
					else
					{
						Dust.NewDustPerfect(player.Center + new Vector2(player.width * Main.rand.NextFloat(0.0f, 1.0f), player.height / 2 * player.gravDir), DustID.Electric, -velocity.RotatedByRandom(MathHelper.Pi), 0, default, Main.rand.NextFloat(0.5f, 1.0f));
						
						if (!Main.dedServ)
						{
							Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Unit() * 30;
							ParticleHandler.SpawnParticle(new ImpactLine(randomPos, Projectile.Center.DirectionTo(randomPos) * Main.rand.NextFloat(1.0f, 3.0f), Color.Lerp(Color.White, Color.Blue, Main.rand.NextFloat(0.0f, 1.0f)), new Vector2(0.5f, Main.rand.NextFloat(0.5f, 1.5f)), 20) 
								{ Rotation = randomPos.AngleTo(Projectile.Center) });
						}
					}
				}

				int damage = (int)(fallDistance * 80);
				Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<EnergyStomp_Explosion>(), damage, 5, player.whoAmI, fallDistance);

				for (int i = 0; i < 2; i++)
					Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + new Vector2(0, 20 * player.gravDir), Vector2.UnitX * fallDistance * ((i > 0) ? -10 : 10), ModContent.ProjectileType<EnergyShockwave>(), damage, fallDistance / 2, player.whoAmI);

				MyPlayer modPlayer = player.GetModPlayer<MyPlayer>();
				modPlayer.Shake += Math.Min(modPlayer.Shake + (int)(fallDistance * 15f), 30);

				if (!Main.dedServ)
					SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/Slash1"), Projectile.Center);

				Landed = true;
			}
			if (!Landed)
			{
				float tolerance = 0.15f;
				Projectile.rotation = MathHelper.Clamp(player.velocity.X * -0.025f, -tolerance, tolerance) + 1.57f + ((player.gravDir == -1.0) ? MathHelper.Pi : 0);

				Dust dust = Dust.NewDustPerfect(player.position + new Vector2(player.width * Main.rand.NextFloat(0.0f, 1.0f), player.height), DustID.Electric, player.velocity * Main.rand.NextFloat(0.1f, 0.3f), 0, default, 0.5f);
				dust.noGravity = true;

				Projectile.timeLeft = 2;
			}

			//Fade in
			if (Projectile.alpha > 0)
				Projectile.alpha -= 255 / 20;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => Main.player[Projectile.owner].GetModPlayer<MyPlayer>().Shake += 5;

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
			=> overPlayers.Add(index);

		public override bool PreDraw(ref Color lightColor)
		{
			#region shader
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			Vector4 colorMod = new Color(20, 72, 230, Projectile.alpha).ToVector4();
			SpiritMod.StarjinxNoise.Parameters["colorMod"].SetValue(colorMod * Projectile.Opacity);
			SpiritMod.StarjinxNoise.Parameters["noise"].SetValue(Mod.Assets.Request<Texture2D>("Textures/vnoise").Value);
			SpiritMod.StarjinxNoise.Parameters["opacity2"].SetValue(0.25f * Projectile.Opacity);
			SpiritMod.StarjinxNoise.Parameters["counter"].SetValue(0);
			SpiritMod.StarjinxNoise.CurrentTechnique.Passes[2].Apply();

			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = SpriteEffects.None;
			float rotation = Projectile.rotation + ((player.gravDir < 0f) ? MathHelper.Pi : 0);

			Main.spriteBatch.Draw(Mod.Assets.Request<Texture2D>("Effects/Masks/Extra_49").Value, Projectile.Center - new Vector2(0, 56) - Main.screenPosition, null, new Color(30, 130, 200) * Projectile.Opacity, rotation, new Vector2(50, 50), Projectile.scale * new Vector2(1.5f, 0.2f), effects, 0f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);
			#endregion
			return false;
		}
	}

	public class EnergyStomp_Explosion : ModProjectile, IDrawAdditive
	{
		private float Strength
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		private readonly int timeLeftMax = 40;

		public override string Texture => SpiritMod.EMPTY_TEXTURE;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Energy Stomp");
		public override void SetDefaults()
		{
			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.scale = .5f;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = timeLeftMax;
			Projectile.friendly = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			Projectile.scale += Strength * 0.05f;
			Projectile.alpha += 255 / (timeLeftMax + 10);
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
		{
			Vector2 dimensions = new Vector2(96 * Projectile.scale);
			hitbox = new Rectangle((int)(Projectile.Center.X - (dimensions.X / 2)), (int)(Projectile.Center.Y - (dimensions.Y / 2)), (int)dimensions.X, (int)dimensions.Y);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
			=> overPlayers.Add(index);

		public void AdditiveCall(SpriteBatch sB, Vector2 screenPos)
		{
			Texture2D bloom = Mod.Assets.Request<Texture2D>("Effects/Masks/CircleGradient").Value;
			Texture2D ring = Mod.Assets.Request<Texture2D>("Effects/WispSwitchGlow2").Value;

			Vector2 position = Projectile.Center + new Vector2(0, 18) - screenPos;

			sB.Draw(bloom, position, null, Color.Cyan * Projectile.Opacity, Projectile.rotation, bloom.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
			for (int i = 0; i < 3; i++)
			{
				Color drawColor = new Color(255 - (i * 50), 255 - (i * 25), 255) * Projectile.Opacity;
				sB.Draw(ring, position, null, drawColor, Projectile.rotation, ring.Size() / 2, Projectile.scale * 0.8f, SpriteEffects.None, 0);
			}
		}
	}
}