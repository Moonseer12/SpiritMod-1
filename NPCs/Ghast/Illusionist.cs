﻿using Microsoft.Xna.Framework;
using SpiritMod.Items.Accessory;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Ghast
{
	public class Illusionist : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ghast");
			Main.npcFrameCount[NPC.type] = 5;
			NPCHelper.ImmuneTo(this, BuffID.Poisoned, BuffID.Venom, BuffID.Confused);

			var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Velocity = 1f,
				Position = new Vector2(0f, 20f),
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 90;
			NPC.lifeMax = 155;
			NPC.defense = 14;
			NPC.damage = 25;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 1200f;
			NPC.knockBackResist = 0.75f;
			NPC.noGravity = true;
			NPC.netAlways = true;
			NPC.chaseable = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;

			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.GhastBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
				new FlavorTextBestiaryInfoElement("A master of illusionary spiritual arts. They will summon violent apparitions in an attempt to distract and overwhelm you."),
			});
		}

		int frame = 0;
		bool aggroed = false;
		int moveSpeed = 0;
		int moveSpeedY = 0;
		float HomeY = 100f;

		public override void AI()
		{
			NPC.spriteDirection = NPC.direction;
			Player target = Main.player[NPC.target];
			float distance = NPC.DistanceSQ(target.Center);
			if (distance < 200 * 200)
			{
				if (!aggroed)
					SoundEngine.PlaySound(SoundID.Zombie53, NPC.Center);
				aggroed = true;
			}

			if (!aggroed)
			{
				if (NPC.localAI[0] == 0f)
				{
					NPC.localAI[0] = NPC.Center.Y;
					NPC.netUpdate = true; //localAI probably isnt affected by this... buuuut might as well play it safe
				}
				if (NPC.Center.Y >= NPC.localAI[0])
				{
					NPC.localAI[1] = -1f;
					NPC.netUpdate = true;
				}
				if (NPC.Center.Y <= NPC.localAI[0] - 2f)
				{
					NPC.localAI[1] = 1f;
					NPC.netUpdate = true;
				}
				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y + 0.009f * NPC.localAI[1], -.25f, .25f);
			}
			else
			{
				Player player = Main.player[NPC.target];

				if (NPC.Center.X >= player.Center.X && moveSpeed >= -30) // flies to players x position
					moveSpeed--;

				if (NPC.Center.X <= player.Center.X && moveSpeed <= 30)
					moveSpeed++;

				NPC.velocity.X = moveSpeed * 0.08f;

				if (NPC.Center.Y >= player.Center.Y - HomeY && moveSpeedY >= -20) //Flies to players Y position
				{
					moveSpeedY--;
					HomeY = 165f;
				}

				if (NPC.Center.Y <= player.Center.Y - HomeY && moveSpeedY <= 20)
					moveSpeedY++;

				NPC.velocity.Y = moveSpeedY * 0.1f;
				if (Main.rand.NextBool(250))
					HomeY = -25f;

				++NPC.ai[0];

				if (NPC.CountNPCS(ModContent.NPCType<IllusionistSpectre>()) < 3)
				{
					if (NPC.ai[0] == 240 || NPC.ai[0] == 480 || NPC.ai[0] == 720)
					{
						SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
						SoundEngine.PlaySound(SoundID.Zombie53, NPC.Center);

						if (Main.netMode != NetmodeID.MultiplayerClient)
							NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width / 2, (int)NPC.Center.Y - 16, ModContent.NPCType<IllusionistSpectre>(), 0, 0, 0, 0, 0, 255);

						switch (Main.rand.Next(3))
						{
							case 0:
								//DustHelper.DrawStar(new Vector2(npc.Center.X, npc.Center.Y - 30), 180, pointAmount: 5, mainSize: 2.25f * ScaleMult, dustDensity: 2, pointDepthMult: 0.3f, noGravity: true);
								DustHelper.DrawTriangle(new Vector2(NPC.Center.X, NPC.Center.Y - 30), 180, 3);
								break;
							case 1:
								DustHelper.DrawTriangle(new Vector2(NPC.Center.X, NPC.Center.Y - 30), 180, 3);
								break;
							case 2:
								DustHelper.DrawDiamond(new Vector2(NPC.Center.X, NPC.Center.Y - 30), 180, 3);
								break;
						}
					}

					if (NPC.ai[0] >= 230 && NPC.ai[0] <= 250 || NPC.ai[0] >= 470 && NPC.ai[0] <= 485 || NPC.ai[0] >= 710 && NPC.ai[0] <= 725)
						frame = 4;
				}

				if (NPC.ai[0] >= 720)
					NPC.ai[0] = 0;
			}

			Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.122f, .5f, .48f);
		}

		public override Color? GetAlpha(Color lightColor) => new Color(189, 195, 184);

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			if (!aggroed)
				SoundEngine.PlaySound(SoundID.Zombie53, NPC.Center);
			aggroed = true;
		}

		public override void FindFrame(int frameHeight)
		{
			if (!aggroed)
			{
				if (++NPC.frameCounter == 4)
				{
					NPC.frameCounter = 0;

					if (++frame >= 4)
						frame = 1;
				}
			}
			else
			{
				if (++NPC.frameCounter == 4)
				{
					NPC.frameCounter = 0;

					if (++frame >= 3)
						frame = 0;
				}
			}

			NPC.frame.Y = frameHeight * frame;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (!NPC.downedBoss3)
				return 0;

			if (NPC.downedPlantBoss)
				return spawnInfo.Player.ZoneDungeon && NPC.CountNPCS(ModContent.NPCType<Illusionist>()) < 1 ? 0.005f : 0f;
			return spawnInfo.Player.ZoneDungeon && NPC.CountNPCS(ModContent.NPCType<Illusionist>()) < 1 ? 0.09f : 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.AddCommon(ItemID.GoldenKey, 65);
			npcLoot.AddCommon(ItemID.Nazar, 75);
			npcLoot.AddCommon(ItemID.TallyCounter, 100);
			npcLoot.AddCommon(ItemID.BoneWand, 250);
			npcLoot.AddCommon<IllusionistEye>(20);
			npcLoot.AddCommon<ForbiddenKnowledgeTome>(25);
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 30; k++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Obsidian, 2.5f * hitDirection, -2.5f, 0, Color.White, 0.7f);
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.DungeonSpirit, 2.5f * hitDirection, -2.5f, 0, default, .34f);
			}

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Illusionist1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Illusionist2").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Illusionist3").Type, 1f);
			}
		}
	}
}