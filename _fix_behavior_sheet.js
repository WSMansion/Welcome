const XLSX = require('./_ConfigExcels/node_modules/xlsx');
const path = require('path');
const file = '_ConfigExcels/Datas/shop_behavior.xlsx';
const wb = XLSX.readFile(file);
const ws = wb.Sheets[wb.SheetNames[0]];
const rows = XLSX.utils.sheet_to_json(ws, {header:1});
const headers = rows[0];
const types = ['##type'];
for (let i = 1; i < headers.length; i++) {
  const name = headers[i];
  types.push(name.endsWith('_chance') || name.endsWith('_count') || name.endsWith('_threshold') || name.endsWith('_penalty') || name.endsWith('_item') ? 'int' : name.includes('seconds') || name.includes('radius') || name.includes('margin') || name.includes('interval') || name.includes('multiplier') ? 'float' : name.endsWith('_count') ? 'int' : 'int');
}
const explicit = {
  notice_radius:'float', notice_reset_margin:'float', glance_min_seconds:'float', glance_max_seconds:'float',
  notice_thought_bubble_chance:'int', browse_thought_bubble_chance:'int', unhappy_bubble_chance:'int', unhappy_bubble_seconds:'float',
  display_bonus_per_item:'int', hurry_entry_penalty:'int', social_check_interval:'float', greeting_radius:'float', greeting_chance:'int',
  greeting_min_seconds:'float', greeting_max_seconds:'float', conversation_radius:'float', conversation_chance:'int', conversation_min_seconds:'float', conversation_max_seconds:'float',
  door_keepout_radius:'float', social_cooldown_seconds:'float', customer_count:'int', customer_spawn_interval:'float', passerby_count:'int', passerby_spawn_interval:'float',
  empty_display_leave_threshold:'int', empty_display_bubble_seconds:'float', passerby_lane_count:'int', passerby_right_to_left_chance:'int', passerby_speed_variation_percent:'int', greeting_happy_chance:'int',
  greeting_recognition_radius:'float', greeting_approach_speed_multiplier:'float', greeting_approach_max_seconds:'float', social_reaction_seconds:'float', social_farewell_seconds:'float'
};
const fixed = ['##type', ...headers.slice(1).map(name => explicit[name])];
if (fixed.includes(undefined)) throw new Error('missing type ' + headers.filter(name => name !== '##var' && !explicit[name]).join(','));
rows[1] = fixed;
const out = XLSX.utils.aoa_to_sheet(rows);
wb.Sheets[wb.SheetNames[0]] = out;
XLSX.writeFile(wb, file);
console.log(fixed.length, headers.length);
