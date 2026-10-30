# Ancient temple integration

The two playable scenes use an ancient temple courtyard reached from the existing eastern causeway. The existing sanctuary geometry remains in place. The original ancestor and all six magic exchange ancestors, together with their memorial stones, are placed within the new courtyard.

## Supplied sources

- \`Desktop/MODEL/A301-1款祭祀古庙场景/Environment 3 Attempt 3.blend\`: ritual stone arches, pillars, gate, carved statue, offering posts and floor.
- \`Desktop/MODEL/A427-寺庙场景/Free Chinese Temple Pack.blend\`: main hall, side shrines, timber platform and stairs.
- \`Desktop/MODEL/A242-一组1陶瓷/陶瓷节点材质.blend\`: eight UV mapped ceramic objects. Their procedural colors were baked to portable albedo maps.

The original A301 and A427 Blender scenes are too large for the Quest target. Selected geometry was exported as FBX with reduced polygon counts and 512–1024 pixel textures. Source Blender files remain in \`Desktop/MODEL\`; the imported Unity models, texture maps, materials and ceramic prefabs are in this folder.

The last two magic exchange trees offer the eight A242 ceramics. A retrieved vessel uses the existing held item, throw, pickup and consume interactions.

To rebuild this authored addition after modifying either scene, use **SKY → Integrate A301 A427 temples and A242 ceramics**. The command replaces only the generated ancient temple root, updates the seven ancestors and memorials, and saves both scenes.
