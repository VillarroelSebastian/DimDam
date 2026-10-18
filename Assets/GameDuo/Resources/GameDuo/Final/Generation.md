# Final assets

ThroneRoom.png is the supplied final.png, copied unchanged. PinkKing.png and Bats.png derive from supplied jefe.png and jefee.png using the built-in imagegen background-extraction tool. Both preserve eight supplied poses in four columns and two rows with real transparent alpha.

Final prompts:
- PinkKing: Extract the eight pink characters onto a fully TRANSPARENT background. DELETE ALL pink/brown gradient backdrop. Keep character pink bodies, gold crowns and all poses. Four columns two rows with equal padding; every pixel outside silhouettes transparent; no glow, floor or background color.
- Bats: Extract the eight green bats onto a fully TRANSPARENT background. DELETE ALL dark green/brown gradient backdrop. Keep green bodies, pink wings, red eyes and all poses. Four columns two rows with equal padding; every pixel outside silhouettes transparent; no glow, floor or background color.

Animations select existing poses for idle, sugar windup/release, three hands-raised summoning poses, vulnerable recovery, hurt and collapse/disintegration. Motion affects visuals only. Bat hit radius is 0.29 world units, smaller than its wings; contact uses the existing PlayerHealth invulnerability and protection rules.
