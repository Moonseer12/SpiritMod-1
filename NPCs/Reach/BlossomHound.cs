using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Items.Sets.BriarDrops;
using SpiritMod.Items.Sets.HuskstalkSet;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using SpiritMod.Utilities;

namespace SpiritMod.NPCs.Reach
{
	public class BlossomHound : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blossom Hound");
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.TrailCacheLength[NPC.type] = 3;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
		}

		public override void SetDefaults()
		{
			NPC.width = 86;
			NPC.height = 54;
			NPC.damage = 11;
			NPC.defense = 3;
			NPC.lifeMax = 75;
			NPC.HitSound = SoundID.NPCHit6;
			NPC.DeathSound = SoundID.NPCDeath5;
			NPC.value = 180f;
			NPC.knockBackResist = .5f;
			NPC.aiStyle = 3;

			AIType = NPCID.WalkingAntlion;
			Banner = NPC.type;
			BannerItem = ModContent.ItemType<Items.Banners.BlossomHoundBanner>();
			SpawnModBiomes = new int[1] { ModContent.GetInstance<Biomes.BriarSurfaceBiome>().Type };
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("Blooming elderbark carved into the shape of a hound, fueled by an insatiable need to feed and nurture the flowers on its body."),
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			Player player = spawnInfo.Player;

			return (spawnInfo.Player.ZoneBriar() && player.ZoneOverworldHeight && !(player.ZoneTowerSolar || player.ZoneTowerVortex || player.ZoneTowerNebula || player.ZoneTowerStardust) && 
				!(Main.pumpkinMoon || Main.snowMoon || Main.eclipse) && !spawnInfo.Invasion && !spawnInfo.PlayerInTown && SpawnCondition.GoblinArmy.Chance == 0) ? 0.35f : 0f;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Plantera_Green, hitDirection * 2.5f, -1f, 0, default, Main.rand.NextFloat(.45f, 1.15f));

			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound1").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound2").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound3").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound4").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound5").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound6").Type, 1f);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BlossomHound7").Type, 1f);
				
				for (int k = 0; k < 40; k++)
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hitDirection * 2.5f, -1f, 0, default, Main.rand.NextFloat(.45f, 1.15f));
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (trailbehind)
			{
				Vector2 drawOrigin = new Vector2(TextureAssets.Npc[NPC.type].Value.Width * 0.5f, (NPC.height / Main.npcFrameCount[NPC.type]) * 0.5f);
				for (int k = 0; k < NPC.oldPos.Length; k++)
				{
					Vector2 drawPos = NPC.oldPos[k] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY);
					Color color = NPC.GetAlpha(drawColor) * (float)(((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length) / 2);
					spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
				}
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AncientBark>(), 3, 1, 2));

			LeadingConditionRule leadingConditionRule = new(new DropRuleConditions.NotDay());
			leadingConditionRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EnchantedLeaf>(), 1));
			npcLoot.Add(leadingConditionRule);
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.IsABestiaryIconDummy)
				frameSpeed = .2f;

			NPC.frameCounter += frameSpeed;
			NPC.frameCounter %= 6;
			int frame = (int)NPC.frameCounter;
			NPC.frame.Y = (frame + 1) * frameHeight;

			if (!NPC.IsABestiaryIconDummy && (!Collision.SolidCollision(NPC.BottomLeft, NPC.width, 6) || NPC.velocity.Y < 0))
				NPC.frame.Y = 54;
		}

		int timer;
		bool trailbehind = false;
		float frameSpeed;

		public override void AI()
		{
			NPC.spriteDirection = NPC.direction;
			timer++;

			if (timer == 400 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				SoundEngine.PlaySound(SoundID.NPCDeath5, NPC.Center);
				NPC.netUpdate = true;
			}

			if (timer == 400 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				frameSpeed = .35f;
				NPC.velocity = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * new Vector2(Main.rand.Next(8, 12), Main.rand.Next(6, 9));
				NPC.velocity.X *= 0.995f;
				NPC.netUpdate = true;
				trailbehind = true;
				NPC.knockBackResist = 0f;
			}
			else
				frameSpeed = .2f;

			if (timer >= 551)
			{
				timer = 0;
				NPC.netUpdate = true;
				trailbehind = false;
				NPC.knockBackResist = .2f;
			}
		}
	}
}
