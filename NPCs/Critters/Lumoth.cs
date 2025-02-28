using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Items.Consumable;
using SpiritMod.Items.Material;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;

namespace SpiritMod.NPCs.Critters
{
	public class Lumoth : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lumoth");
			Main.npcFrameCount[NPC.type] = 4;
			Main.npcCatchable[NPC.type] = true;
			NPCID.Sets.CountsAsCritter[Type] = true;
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 5;
			NPC.dontCountMe = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath4;
			NPC.catchItem = (short)ModContent.ItemType<LumothItem>();
			NPC.knockBackResist = .45f;
			NPC.aiStyle = 64;
			NPC.npcSlots = 0;
			NPC.noGravity = true;
			AIType = NPCID.Firefly;
			NPC.dontTakeDamageFromHostiles = false;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				new FlavorTextBestiaryInfoElement("The bulb only stays on while the Lumoth flaps its wings."),
			});
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => GlowmaskUtils.DrawNPCGlowMask(spriteBatch, NPC, ModContent.Request<Texture2D>("SpiritMod/NPCs/Critters/Lumoth_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, screenPos);

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server) {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Lumoth1").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Lumoth").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Lumoth3").Type);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Lumoth4").Type);
				NPC.position.X = NPC.position.X + (float)(NPC.width / 2);
				NPC.position.Y = NPC.position.Y + (float)(NPC.height / 2);
				NPC.width = 32;
				NPC.height = 32;
				NPC.position.X = NPC.position.X - (float)(NPC.width / 2);
				NPC.position.Y = NPC.position.Y - (float)(NPC.height / 2);
				for (int num621 = 0; num621 < 20; num621++) {
					int num622 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.GoldCoin, 0f, 0f, 100, default, 1f);
					Main.dust[num622].velocity *= .1f;
					if (Main.rand.NextBool(2)) {
						Main.dust[num622].scale = 0.9f;
					}
				}
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.Player.ZoneDungeon || spawnInfo.Player.ZoneSnow)
				return 0f;
			if (spawnInfo.PlayerSafe)
				return SpawnCondition.Cavern.Chance * 0.4f;
			return SpawnCondition.Cavern.Chance * 0.0323f;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 0.15f;
			NPC.frameCounter %= Main.npcFrameCount[NPC.type];
			int frame = (int)NPC.frameCounter;
			NPC.frame.Y = frame * frameHeight;
		}

		public override void AI() => Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), .4f, .4f, .4f);
		public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<Brightbulb>(3);
	}
}
