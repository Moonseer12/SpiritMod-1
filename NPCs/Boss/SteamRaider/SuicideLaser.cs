using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Boss.SteamRaider
{
	public class SuicideLaser : ModNPC
	{
		private int timer = 0;
		private float alphaCounter;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Laser Bomber");
			NPCHelper.ImmuneTo(this, BuffID.Poisoned, BuffID.Venom);

			NPCID.Sets.TrailCacheLength[NPC.type] = 3;
			NPCID.Sets.TrailingMode[NPC.type] = 0;

			NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new(0) { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 46;
			NPC.damage = 44;
			NPC.defense = 4;
			NPC.noGravity = true;
			NPC.lifeMax = 5;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.value = 0f;
			NPC.knockBackResist = .0f;
			NPC.noTileCollide = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			float sineAdd = (float)Math.Sin(alphaCounter) + 3;
			Main.spriteBatch.Draw(TextureAssets.Extra[49].Value, (NPC.Center - Main.screenPosition) - new Vector2(-2, 8), null, new Color((int)(7.5f * sineAdd), (int)(16.5f * sineAdd), (int)(18f * sineAdd), 0), 0f, new Vector2(50, 50), 0.25f * (sineAdd + 1), SpriteEffects.None, 0f);

			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY), NPC.frame,
				drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

			return false;
		}

		public override Color? GetAlpha(Color lightColor) => new Color(3, 252, 215, 100);

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.alpha != 255)
				GlowmaskUtils.DrawNPCGlowMask(spriteBatch, NPC, Mod.Assets.Request<Texture2D>("NPCs/Boss/SteamRaider/LaserBase_Glow").Value, screenPos);
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 20; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 2.5f * hitDirection, -2.5f, 117, new Color(0, 255, 142), .6f);
			}
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server) {
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SoundEngine.PlaySound(SoundID.NPCDeath44, NPC.Center);
				SoundEngine.PlaySound(SoundID.NPCHit4, NPC.Center);
				for (int i = 0; i < 40; i++) {
					int num = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.UltraBrightTorch, 0f, -2f, 117, new Color(0, 255, 142), .6f);
					Main.dust[num].noGravity = true;
					Dust dust = Main.dust[num];
					dust.position.X += ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
					dust.position.Y += ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
					if (Main.dust[num].position != NPC.Center) {
						Main.dust[num].velocity = NPC.DirectionTo(Main.dust[num].position) * 6f;
					}
				}
			}
		}

		public override void AI()
		{
			alphaCounter += .08f;
			timer++;
			if (timer >= 90) {
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SoundEngine.PlaySound(SoundID.NPCDeath44, NPC.Center);
				SoundEngine.PlaySound(SoundID.NPCHit4, NPC.Center);
				for (int i = 0; i < 40; i++) {
					int num = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, -2f, 117, new Color(0, 255, 142), .6f);
					Main.dust[num].noGravity = true;
					Dust dust = Main.dust[num];
					dust.position.X += (Main.rand.Next(-50, 51) / 20) - 1.5f;
					dust.position.Y += (Main.rand.Next(-50, 51) / 20) - 1.5f;
					if (Main.dust[num].position != NPC.Center) {
						Main.dust[num].velocity = NPC.DirectionTo(Main.dust[num].position) * 6f;
					}
				}
				NPC.active = false;
			}
			NPC.TargetClosest(true);
			float speed = 16f;
			float acceleration = 0.16f;
			Vector2 vector2 = new Vector2(NPC.position.X + NPC.width * 0.5F, NPC.position.Y + NPC.height * 0.5F);
			float xDir = Main.player[NPC.target].position.X + (Main.player[NPC.target].width * 0.5F) - vector2.X;
			float yDir = Main.player[NPC.target].position.Y + (Main.player[NPC.target].height * 0.5F) - vector2.Y;
			float length = (float)Math.Sqrt(xDir * xDir + yDir * yDir);

			float num10 = speed / length;
			xDir *= num10;
			yDir *= num10;
			if (NPC.velocity.X < xDir) {
				NPC.velocity.X = NPC.velocity.X + acceleration;
				if (NPC.velocity.X < 0 && xDir > 0)
					NPC.velocity.X = NPC.velocity.X + acceleration;
			}
			else if (NPC.velocity.X > xDir) {
				NPC.velocity.X = NPC.velocity.X - acceleration;
				if (NPC.velocity.X > 0 && xDir < 0)
					NPC.velocity.X = NPC.velocity.X - acceleration;
			}

			if (NPC.velocity.Y < yDir) {
				NPC.velocity.Y = NPC.velocity.Y + acceleration;
				if (NPC.velocity.Y < 0 && yDir > 0)
					NPC.velocity.Y = NPC.velocity.Y + acceleration;
			}
			else if (NPC.velocity.Y > yDir) {
				NPC.velocity.Y = NPC.velocity.Y - acceleration;
				if (NPC.velocity.Y > 0 && yDir < 0)
					NPC.velocity.Y = NPC.velocity.Y - acceleration;
			}
			NPC.noTileCollide = true;
			NPC.localAI[0] += 1f;
			if (NPC.localAI[0] == 12f) {
				NPC.localAI[0] = 0f;
				for (int j = 0; j < 12; j++) {
					Vector2 vector21 = Vector2.UnitX * -NPC.width / 2f;
					vector21 += -Utils.RotatedBy(Vector2.UnitY, ((float)j * 3.141591734f / 6f), default) * new Vector2(8f, 16f);
					vector21 = Utils.RotatedBy(vector21, (NPC.rotation - 1.57079637f), default);
					int num8 = Dust.NewDust(NPC.Center, 0, 0, DustID.Electric, 0f, 0f, 160, default, 1f);
					Main.dust[num8].scale = .8f;
					Main.dust[num8].noGravity = true;
					Main.dust[num8].position = NPC.Center;
					Main.dust[num8].velocity = NPC.velocity * 0.1f;
					Main.dust[num8].velocity = Vector2.Normalize(NPC.Center - NPC.velocity * 3f - Main.dust[num8].position) * 1.25f;
				}
			}
			Vector2 direction9 = Main.player[NPC.target].Center - NPC.Center;
			direction9.Normalize();
			NPC.rotation = direction9.ToRotation() + 1.57f + 3.14f;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			NPC.life = 0;
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			SoundEngine.PlaySound(SoundID.NPCDeath44, NPC.Center);
			SoundEngine.PlaySound(SoundID.NPCHit4, NPC.Center);
			for (int i = 0; i < 40; i++) {
				int num = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, 0f, -2f, 117, new Color(0, 255, 142), .6f);
				Main.dust[num].noGravity = true;
				Dust dust = Main.dust[num];
				dust.position.X += ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
				Dust expr_92_cp_0 = Main.dust[num];
				expr_92_cp_0.position.Y += ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
				if (Main.dust[num].position != NPC.Center) {
					Main.dust[num].velocity = NPC.DirectionTo(Main.dust[num].position) * 6f;
				}
			}
		}
	}
}
