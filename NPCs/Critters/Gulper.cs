using SpiritMod.Items.Consumable;
using SpiritMod.Items.Consumable.Fish;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace SpiritMod.NPCs.Critters
{
	public class Gulper : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gulper");
			Main.npcFrameCount[NPC.type] = 6;
			Main.npcCatchable[NPC.type] = true;
			NPCID.Sets.CountsAsCritter[Type] = true;

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Position = new Vector2(0, 6f), };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
		}

		public override void SetDefaults()
		{
			NPC.width = 24;
			NPC.height = 16;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.dontCountMe = true;
			NPC.lifeMax = 5;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.catchItem = (short)ModContent.ItemType<GulperItem>();
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = .35f;
			NPC.aiStyle = 16;
			NPC.noGravity = true;
			NPC.npcSlots = 0;
			AIType = NPCID.Goldfish;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
				new FlavorTextBestiaryInfoElement("These small, domesticated fish are made of a springy substance, are often sold at festivals, and have an extremely short life expectancy."),
			});
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.15f;
			NPC.frameCounter %= Main.npcFrameCount[NPC.type];
			int frame = (int)NPC.frameCounter;
			NPC.frame.Y = frame * frameHeight;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server) {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Gulper1").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Gulper2").Type);
			}
		}

		private int Counter;
		public override void AI()
		{
			NPC.spriteDirection = NPC.direction;
			Counter++;
			if (Counter == 100)
			{
				NPC.velocity.Y *= 10.0f;
				NPC.velocity.X *= 4.0f;
			}

			if (Counter >= 200)
				Counter = 0;

			Player target = Main.player[NPC.target];
			if (NPC.DistanceSQ(target.Center) < 65 * 65 && target.wet && NPC.wet)
			{
				Vector2 vel = NPC.DirectionFrom(target.Center) * 4.5f;
				NPC.velocity = vel;
				NPC.rotation = NPC.velocity.X * .06f;
				if (target.position.X > NPC.position.X)
				{
					NPC.direction = NPC.spriteDirection = -1;
					NPC.netUpdate = true;
				}
				else if (target.position.X < NPC.position.X)
				{
					NPC.direction = NPC.spriteDirection = 1;
					NPC.netUpdate = true;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Rectangle drawFrame = NPC.frame with { Height = NPC.frame.Height - 2 };
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), drawFrame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.PlayerSafe && spawnInfo.Water)
				return 0.5f;
			return SpawnCondition.OceanMonster.Chance * 0.31f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<RawFish>(2);
	}
}
