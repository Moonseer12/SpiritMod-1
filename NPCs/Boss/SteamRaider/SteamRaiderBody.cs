﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Projectiles;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Boss.SteamRaider
{
	public class SteamRaiderBody : ModNPC
	{
		private NPC Head => Main.npc[(int)NPC.ai[2]];
		public bool Exposed => NPC.localAI[0] == 1;

		public override void SetStaticDefaults()
		{
			NPCHelper.BuffImmune(Type, true);
			DisplayName.SetDefault("Starplate Voyager");

			NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new(0) { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
		}

		public override void SetDefaults()
		{
			NPC.width = 36;              
            NPC.height = 36;             
            NPC.damage = 35;
            NPC.defense = 8;
            NPC.lifeMax = 2;
            NPC.knockBackResist = 0.0f;
            NPC.behindTiles = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.noGravity = true;
            NPC.dontCountMe = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.npcSlots = 1f;
            NPC.boss = true;
			NPC.aiStyle = -1;
            NPC.alpha = 200;

			Music = MusicID.Boss3;
			NPC.dontCountMe = true;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = (int)(NPC.lifeMax * (Main.masterMode ? 0.85f : 1.0f) * 0.6f * bossLifeScale);
			NPC.damage = (int)(NPC.damage * 0.65f);
		}

		public override void SendExtraAI(BinaryWriter writer) => writer.Write(NPC.localAI[0]);
		public override void ReceiveExtraAI(BinaryReader reader) => NPC.localAI[0] = reader.ReadSingle();

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => Head.ai[2] == 1 && NPC.ai[3] < 100;

		public override bool PreAI()
		{
			int exposedBodies = Main.npc.Where(x => x.active && (x.type == ModContent.NPCType<SteamRaiderBody>() || x.type == ModContent.NPCType<SteamRaiderBody2>()) && x.localAI[0] > 0).Count();
			
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (exposedBodies < 7 && Main.rand.NextBool(50) || (++NPC.localAI[1] % 60 == 0 && Main.rand.NextBool(50))) {
					if (!Exposed) {
						NPC.localAI[0] = 1;
						NPC.netUpdate = true;
					}
				}
				if (exposedBodies > 10 || (NPC.localAI[1] % 60 == 0 && Main.rand.NextBool(50))) {
					if (Exposed) {
						NPC.localAI[0] = 0;
						NPC.netUpdate = true;
					}
				}
				if (NPC.localAI[2] >= 601) {
					NPC.localAI[2] = 0f;
				}
			}
			if (Exposed) {
				NPC.defense = 8;
				NPC.dontTakeDamage = false;
			}
			else {
				NPC.defense = 9999;
				NPC.dontTakeDamage = true;
			}

			Player player = Main.player[NPC.target];
			Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0f, 0.075f, 0.25f);
			if (Head.ai[2] == 1) {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					if (--NPC.ai[3] == 0) {
						SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
						NPC.TargetClosest(true);
						NPC.netUpdate = true;
						if (Main.rand.NextBool(2)) {
							float num941 = 1f; //speed
							Vector2 vector104 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)(NPC.height / 2));
							float num942 = player.position.X + (float)player.width * 0.5f - vector104.X + (float)Main.rand.Next(-20, 21);
							float num943 = player.position.Y + (float)player.height * 0.5f - vector104.Y + (float)Main.rand.Next(-20, 21);
							float num944 = (float)Math.Sqrt((double)(num942 * num942 + num943 * num943));
							num944 = num941 / num944;
							num942 *= num944;
							num943 *= num944;
							num942 += (float)Main.rand.Next(-10, 11) * 0.0125f;
							num943 += (float)Main.rand.Next(-10, 11) * 0.0125f;
							int num946 = ModContent.ProjectileType<Starshock>();
							vector104.X += num942 * 4f;
							vector104.Y += num943 * 2.5f;
							int num947 = Projectile.NewProjectile(NPC.GetSource_FromAI(), vector104.X, vector104.Y, num942, num943, num946, NPCUtils.ToActualDamage(30, 1.25f, 1.75f), 0f, Main.myPlayer, 0f, 0f);
							Main.projectile[num947].timeLeft = 350;
						}
					}
				}
			}
			else
				NPC.ai[3] = Main.rand.Next(175, 190);

			if (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].life <= Main.npc[(int)NPC.ai[1]].lifeMax * .2f) {
				NPC.life = 0;
				NPC.HitEffect(0, 10.0);
				NPC.active = false;
			}
			if (Main.npc[(int)NPC.ai[1]].alpha < 128) {
				if (NPC.alpha != 0) {
					for (int num934 = 0; num934 < 2; num934++) {
						int num935 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Electric, 0f, 0f, 100, default, 2f);
						Main.dust[num935].noGravity = true;
						Main.dust[num935].noLight = true;
					}
				}
				NPC.alpha -= 42;
				if (NPC.alpha < 0) {
					NPC.alpha = 0;
				}
			}
			if (NPC.ai[2] > 0) {
				NPC.realLife = (int)NPC.ai[2];
			}

			if (NPC.target < 0 || NPC.target == byte.MaxValue || Main.player[NPC.target].dead) {
				NPC.TargetClosest(true);
			}

			if (Main.player[NPC.target].dead && NPC.timeLeft > 300) {
				NPC.timeLeft = 300;
			}

			if (NPC.ai[1] < (double)Main.npc.Length) {
				Vector2 npcCenter = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);

				float dirX = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - npcCenter.X;
				float dirY = Main.npc[(int)NPC.ai[1]].position.Y + (float)(Main.npc[(int)NPC.ai[1]].height / 2) - npcCenter.Y;

				NPC.rotation = (float)Math.Atan2(dirY, dirX) + 1.57f;

				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

				float dist = (length - (float)NPC.width) / length;
				float posX = dirX * dist;
				float posY = dirY * dist;

				NPC.velocity = Vector2.Zero;
				NPC.position.X = NPC.position.X + posX;
				NPC.position.Y = NPC.position.Y + posY;
			}
			return true;
		}

		public override bool CheckActive() => false;
		public override bool PreKill() => false;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			var pos = NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY);
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, pos, NPC.frame, NPC.GetNPCColorTintedByBuffs(Lighting.GetColor(NPC.Center.ToTileCoordinates())), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Exposed)
			{
				Texture2D tex = TextureAssets.GlowMask[239].Value;
				float timer = (float)(Main.GlobalTimeWrappedHourly % 1.0);

				float scaleOne = timer;
				if (scaleOne > 0.5) scaleOne = 1f - timer;
				else if (scaleOne < 0.0) scaleOne = 0.0f;

				float timerOffset = (float)((timer + 0.5) % 1.0);

				float scaleTwo = timerOffset;
				if (scaleTwo > 0.5) scaleTwo = 1f - timerOffset;
				else if (scaleTwo < 0.0) scaleTwo = 0.0f;

				Rectangle source = tex.Frame(1, 1, 0, 0);
				Vector2 drawOrigin = source.Size() / 2f;
				Vector2 bottom = NPC.Bottom - screenPos + new Vector2(0.0f, -20f);
				Color drawCol = new Color(84, 207, 255) * 1.6f;

				float scaleThree = 1f + timer * 0.75f;
				float scaleFour = 1f + timerOffset * 0.75f;

				Main.spriteBatch.Draw(tex, bottom, source, drawCol, NPC.rotation, drawOrigin, NPC.scale * 0.5f, SpriteEffects.FlipHorizontally, 0.0f);
				Main.spriteBatch.Draw(tex, bottom, source, drawCol * scaleOne, NPC.rotation, drawOrigin, NPC.scale * 0.5f * scaleThree, SpriteEffects.FlipHorizontally, 0.0f);
				Main.spriteBatch.Draw(tex, bottom, source, drawCol * scaleTwo, NPC.rotation, drawOrigin, NPC.scale * 0.5f * scaleFour, SpriteEffects.FlipHorizontally, 0.0f);

				GlowmaskUtils.DrawNPCGlowMask(spriteBatch, NPC, Mod.Assets.Request<Texture2D>("NPCs/Boss/SteamRaider/SteamRaiderBody_Glow").Value, screenPos);
			}
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 5; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, hitDirection, -1f, 0, default, 1f);
			}
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server) {
				SoundEngine.PlaySound(SoundID.Item4, NPC.Center);

				for (int num623 = 0; num623 < 20; num623++) {
					int dust1 = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Electric);

					Main.dust[dust1].velocity *= -1f;
					Main.dust[dust1].noGravity = true;
					Main.dust[dust1].scale *= .8f;
					Vector2 vector2_1 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
					vector2_1.Normalize();
					Vector2 vector2_2 = vector2_1 * (Main.rand.Next(50, 100) * 0.04f);
					Main.dust[dust1].velocity = vector2_2;
					vector2_2.Normalize();
					Vector2 vector2_3 = vector2_2 * 104f;
					Main.dust[dust1].position = (NPC.Center) - vector2_3;
				}

				Vector2 direction = Main.player[NPC.target].Center - NPC.Center;
				direction.Normalize();
				direction.X *= 12;
				direction.Y *= -12f;

				int amountOfProjectiles = Main.rand.Next(1, 2);
				for (int i = 0; i < amountOfProjectiles; ++i) {
					float A = Main.rand.Next(-150, 150) * 0.01f;
					float B = Main.rand.Next(-80, 0) * 0.0f;
					Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center.X, NPC.Center.Y, direction.X + A, direction.Y + B, ModContent.ProjectileType<SteamBodyFallingProj>(), 15, 1, Main.myPlayer, 0, 0);
				}
			}
		}
	}
}