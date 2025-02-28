using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Items.Sets.SlagSet;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritMod.Items.Consumable.Food;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;

namespace SpiritMod.NPCs.HellEater
{
	public class HellEater : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gluttonous Devourer");
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Pixie];
			NPCID.Sets.TrailCacheLength[NPC.type] = 3;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
			NPCHelper.ImmuneTo(this, BuffID.OnFire, BuffID.Confused);
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 46;
			NPC.damage = 30;
			NPC.defense = 16;
			NPC.lifeMax = 120;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = 300f;
			NPC.knockBackResist = .29f;
			NPC.aiStyle = 85;
			NPC.noGravity = true;
			NPC.lavaImmune = true;

			AIType = NPCID.StardustCellBig;
			AnimationType = NPCID.Pixie;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.GluttonousDevourerBanner>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
				new FlavorTextBestiaryInfoElement("You'd best avoid the searing hot jaws of these beasts. Devourers have a notoriously painful bite, but that makes it a sought after material for the bold."),
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => NPC.downedBoss3 ? SpawnCondition.Underworld.Chance * 0.09f : 0f;

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 2.5f * hitDirection, -2.5f, 117, default, .6f);

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 20; i++)
				{
					int num = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, -2f, 117, default, .6f);
					Main.dust[num].noGravity = true;
					Dust dust = Main.dust[num];
					dust.position.X = dust.position.X + ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
					Dust expr_92_cp_0 = Main.dust[num];
					expr_92_cp_0.position.Y = expr_92_cp_0.position.Y + ((float)(Main.rand.Next(-50, 51) / 20) - 1.5f);
					if (Main.dust[num].position != NPC.Center) {
						Main.dust[num].velocity = NPC.DirectionTo(Main.dust[num].position) * 6f;
					}
				}
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

				for(int i = 1; i <= 3; i++)
					Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("DevourerGore" + i).Type);
            }
		}

		int dashtimer;
		public override void SendExtraAI(BinaryWriter writer) => writer.Write(dashtimer);
		public override void ReceiveExtraAI(BinaryReader reader) => dashtimer = reader.ReadInt32();

		public override void AI()
		{
			Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.1f, 0.04f, 0.02f);

			int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch);
			Main.dust[dust].noGravity = true;

			Vector2 direction = Main.player[NPC.target].Center - NPC.Center;
			direction.Normalize();
			NPC.velocity *= 0.98f;

			if (++dashtimer >= 180) //Dash
			{
				dashtimer = 0;
				NPC.netUpdate = true;
				direction.X *= Main.rand.Next(8, 11);
				direction.Y *= Main.rand.Next(8, 11);
				NPC.velocity.X = direction.X;
				NPC.velocity.Y = direction.Y;

				if (Main.netMode != NetmodeID.Server)
					SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 drawOrigin = NPC.frame.Size() / 2;
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			for (int k = 0; k < NPC.oldPos.Length; k++)
			{
				Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + (NPC.Size / 2) + new Vector2(0f, NPC.gfxOffY);
				Color color = NPC.GetAlpha(drawColor) * (float)(((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length) / 2);
				spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
			}
			return true;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.rand.NextBool(3))
				target.AddBuff(BuffID.OnFire, 180);

			target.AddBuff(BuffID.Bleeding, 180);
			if (Main.rand.NextBool(4))
			{
				if (NPC.life <= NPC.lifeMax - 10)
				{
					NPC.life += 10;
					NPC.HealEffect(10, true);
				}
				else if (NPC.life < NPC.lifeMax)
				{
					NPC.HealEffect(NPC.lifeMax - NPC.life, true);
					NPC.life += NPC.lifeMax - NPC.life;
				}
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.AddCommon(ModContent.ItemType<CarvedRock>(), 1, 1, 2);
			npcLoot.AddCommon(ModContent.ItemType<Items.Accessory.HellEater>(), 20);
			npcLoot.AddCommon(ModContent.ItemType<GhostPepper>(), 16);
		}
	}
}
