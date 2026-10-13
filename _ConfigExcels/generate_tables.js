const XLSX = require('xlsx');
const path = require('path');
const fs = require('fs');

const DATAS = path.join(__dirname, 'Datas');

function writeXlsx(filename, sheetName, headers, rows) {
    const ws = XLSX.utils.aoa_to_sheet([headers, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, sheetName);
    const filepath = path.join(DATAS, filename);
    XLSX.writeFile(wb, filepath);
    console.log(`  Created: ${filename}`);
}

// ============================================================
// 1. __enums__.xlsx
// ============================================================
console.log('Creating __enums__.xlsx ...');
{
    const headers = ['##var', 'full_name', 'flags', 'unique', 'comment', 'tags',
        '##var', 'name', 'alias', 'value', 'item_comment', 'item_tags'];
    const rows = [];
    writeXlsx('__enums__.xlsx', 'enums', headers, rows);
}

// ============================================================
// 2. __beans__.xlsx
// ============================================================
console.log('Creating __beans__.xlsx ...');
{
    const headers = ['##var', 'full_name', 'parent', 'valueType', 'alias', 'sep', 'comment', 'tags',
        '##var', 'name', 'type', 'group', 'field_comment', 'field_tags'];
    const rows = [];
    writeXlsx('__beans__.xlsx', 'beans', headers, rows);
}

// ============================================================
// 3. __tables__.xlsx
// ============================================================
console.log('Creating __tables__.xlsx ...');
{
    const headers = ['##var', 'full_name', 'value_type', 'read_schema_from_file', 'index', 'mode', 'group', 'comment', 'input', 'tags', 'output'];
    const rows = [
        ['', 'TbGlobalParam', 'GlobalParam', true, '', 'one', 'c', '全局参数', 'global_param.xlsx', '', ''],
        ['', 'TbMaterial', 'Material', true, 'id', 'map', 'c', '素材表', 'material.xlsx', '', ''],
        ['', 'TbMagic', 'Magic', true, 'id', 'map', 'c', '魔法表', 'magic.xlsx', '', ''],
        ['', 'TbRecipe', 'Recipe', true, 'id', 'map', 'c', '配方表', 'recipe.xlsx', '', ''],
        ['', 'TbDayEvent', 'DayEvent', true, 'id', 'map', 'c', '白天事件表', 'day_event.xlsx', '', ''],
        ['', 'TbCustomer', 'Customer', true, 'id', 'map', 'c', '顾客表', 'customer.xlsx', '', ''],
        ['', 'TbAvgStory', 'AvgStory', true, 'id', 'map', 'c', 'AVG剧情组', 'avg_story.xlsx', '', ''],
        ['', 'TbAvgEvent', 'AvgEvent', true, 'id', 'map', 'c', 'AVG对话事件', 'avg_event.xlsx', '', ''],
        ['', 'TbDisplayCase', 'DisplayCase', true, 'id', 'map', 'c', '陈列柜配置表', 'display_case.xlsx', '', ''],
        ['', 'TbShopBehavior', 'ShopBehavior', true, '', 'one', 'c', '街区行为全局参数', 'shop_behavior.xlsx', '', ''],
        ['', 'TbShopNpcProfile', 'ShopNpcProfile', true, 'id', 'map', 'c', '街区人物行为档案', 'shop_npc_profile.xlsx', '', ''],
    ];
    writeXlsx('__tables__.xlsx', 'tables', headers, rows);
}

// ============================================================
// 4. global_param.xlsx (singleton)
// ============================================================
console.log('Creating global_param.xlsx ...');
{
    const headers = [
        '##var', 'starting_gold', 'ap_per_day', 'magic_per_night', 'night_slots', 'protection_fee', 'protection_fee_day'
    ];
    const typeRow = ['##type', 'int', 'int', 'int', 'int', 'int', 'int'];
    const commentRow = ['##comment', '初始金币', '每日行动力', '每夜魔力', '夜晚时间槽', '保护费', '保护费周期(天)'];
    const dataRow = ['', 100, 10, 10, 5, 50, 7];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, dataRow]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'global_param');
    XLSX.writeFile(wb, path.join(DATAS, 'global_param.xlsx'));
    console.log('  Created: global_param.xlsx');
}

// ============================================================
// 5. material.xlsx
// ============================================================
console.log('Creating material.xlsx ...');
{
    const headers = ['##var', 'id', 'name', 'desc', 'category', 'buy_price', 'icon'];
    const typeRow = ['##type', 'int', 'string', 'string', 'int', 'int', 'string'];
    const commentRow = ['##comment', 'ID', '名称', '描述', '分类', '购买价格', '图标路径'];
    const rows = [
        ['', 1001, '铁锭', '普通的铁块', 1, 5, ''],
        ['', 1002, '铜锭', '暗红色的铜块', 1, 3, ''],
        ['', 1003, '橡木板', '结实的木板', 2, 4, ''],
        ['', 1004, '棉布', '柔软的棉布', 3, 3, ''],
        ['', 1005, '麂皮', '细腻的皮料', 3, 8, ''],
        ['', 1006, '魔力碎晶', '微弱发光的碎片', 4, 15, ''],
        ['', 1007, '艾草', '有驱虫功效的草药', 5, 4, ''],
        ['', 1008, '陶土', '可塑性好的黏土', 6, 2, ''],
        ['', 1009, '玻璃片', '透明的玻璃碎片', 6, 3, ''],
        ['', 1010, '蜡块', '可制作蜡烛', 6, 2, ''],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'material');
    XLSX.writeFile(wb, path.join(DATAS, 'material.xlsx'));
    console.log('  Created: material.xlsx');
}

// ============================================================
// 6. magic.xlsx
// ============================================================
console.log('Creating magic.xlsx ...');
{
    const headers = ['##var', 'id', 'name', 'desc', 'magic_type', 'unlock_day'];
    const typeRow = ['##type', 'int', 'string', 'string', 'int', 'int'];
    const commentRow = ['##comment', 'ID', '名称', '描述', '魔法类型枚举', '解锁天数'];
    const rows = [
        ['', 2001, '发光术', '让物体持续发出柔和光芒', 1, 1],
        ['', 2002, '恒温术', '维持物体温度恒定', 2, 1],
        ['', 2003, '清洁术', '去除污渍和灰尘', 3, 3],
        ['', 2004, '悬浮术', '让物体漂浮减轻重量', 4, 5],
        ['', 2005, '坚固术', '增强物体结构强度', 5, 8],
        ['', 2006, '驱虫术', '驱赶昆虫和害虫', 6, 10],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'magic');
    XLSX.writeFile(wb, path.join(DATAS, 'magic.xlsx'));
    console.log('  Created: magic.xlsx');
}

// ============================================================
// 7. recipe.xlsx
// ============================================================
console.log('Creating recipe.xlsx ...');
{
    const headers = ['##var', 'id', 'product_name', 'desc', 'material_id', 'material_cost',
        'magic_type', 'magic_cost_base', 'base_price', 'need_tag', 'unlock_day'];
    const typeRow = ['##type', 'int', 'string', 'string', 'int', 'int',
        'int', 'int', 'int', 'int', 'int'];
    const commentRow = ['##comment', 'ID', '产品名', '描述', '素材ID', '素材消耗',
        '魔法类型', '基础魔力消耗', '基础售价', '需求标签', '解锁天数'];
    const rows = [
        ['', 3001, '长明灯', '注入发光魔法的油灯，可持续照明', 1010, 1, 1, 1, 15, 1, 1],
        ['', 3002, '保温罐', '注入恒温魔法的瓦罐，保持食物温度', 1008, 1, 2, 1, 18, 2, 1],
        ['', 3003, '百洁布', '注入清洁魔法的麂皮，轻松去污', 1005, 1, 3, 1, 22, 3, 3],
        ['', 3004, '省力滑轮', '注入悬浮魔法的铁制滑轮', 1001, 2, 4, 2, 35, 4, 5],
        ['', 3005, '强化锤', '注入坚固魔法的工匠锤', 1001, 2, 5, 2, 30, 4, 8],
        ['', 3006, '驱虫陶片', '注入驱虫魔法的陶片，保护庄稼', 1008, 1, 6, 1, 12, 6, 10],
        ['', 3007, '夜光玻璃瓶', '发光玻璃容器', 1009, 1, 1, 1, 20, 1, 2],
        ['', 3008, '暖手铜块', '恒温铜块，冬日取暖', 1002, 1, 2, 1, 12, 2, 2],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'recipe');
    XLSX.writeFile(wb, path.join(DATAS, 'recipe.xlsx'));
    console.log('  Created: recipe.xlsx');
}

// ============================================================
// 8. day_event.xlsx
// ============================================================
console.log('Creating day_event.xlsx ...');
{
    const headers = ['##var', 'id', 'title', 'desc', 'event_type', 'ap_cost',
        'gold_reward', 'material_rewards', 'attention_delta', 'support_delta',
        'day_min', 'day_max', 'duration', 'one_time'];
    const typeRow = ['##type', 'int', 'string', 'string', 'int', 'int',
        'int', 'string', 'int', 'int',
        'int', 'int', 'int', 'bool'];
    const commentRow = ['##comment', 'ID', '标题', '描述', '事件类型', 'AP消耗',
        '金币奖励', '素材奖励(id:amt;...)', '关注度变化', '民心变化',
        '最早天数', '最晚天数', '持续天数', '是否一次性'];
    const rows = [
        ['', 4001, '逛市场', '去集市采购日常材料', 1, 1, -5, '1008:2;1010:2', 0, 0, 1, 999, 0, false],
        ['', 4002, '矿区采集', '前往城外矿区收集金属', 1, 2, 0, '1001:2;1002:1', 1, 0, 1, 999, 0, false],
        ['', 4003, '林地采集', '在树林中收集木材和草药', 1, 1, 0, '1003:2;1007:1', 0, 0, 1, 999, 0, false],
        ['', 4004, '黑市探访', '从地下市场打听魔法情报', 2, 2, -10, '', 3, 0, 1, 999, 0, false],
        ['', 4005, '图书馆研究', '在贵族图书馆翻阅文献', 2, 3, -15, '', 5, 0, 3, 999, 0, false],
        ['', 4006, '帮铁匠修工具', '帮助铁匠老赵修理坏掉的工具', 3, 1, 5, '', -1, 3, 1, 999, 0, false],
        ['', 4007, '帮洗衣妇晾衣', '帮王婶儿晾晒大量衣物', 3, 1, 0, '1004:1', -1, 2, 1, 999, 0, false],
        ['', 4008, '莱亚的发现', '莱亚在旧书中发现了一段有趣的记载', 4, 2, 0, '', 0, 0, 2, 5, 0, true],
        ['', 4009, '可疑的目光', '一个穿着讲究的人似乎在观察工坊...', 5, 1, 0, '', 8, 0, 5, 999, 3, false],
        ['', 4010, '布料商人', '一个走街串巷的布匹商人路过', 1, 1, -8, '1004:3;1005:1', 0, 0, 1, 999, 0, false],
        ['', 4011, '冥想练习', '让雀安静冥想稳定魔力', 2, 2, 0, '', 0, 0, 1, 999, 0, false],
        ['', 4012, '讨好巡查', '给巡查组送点小礼物打点关系', 3, 1, -15, '', -10, 0, 1, 999, 0, false],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'day_event');
    XLSX.writeFile(wb, path.join(DATAS, 'day_event.xlsx'));
    console.log('  Created: day_event.xlsx');
}

// ============================================================
// 9. customer.xlsx
// ============================================================
console.log('Creating customer.xlsx ...');
{
    const headers = ['##var', 'id', 'customer_name', 'need_tag', 'need_desc',
        'patience', 'spawn_chance', 'support_delta', 'attention_delta',
        'is_special', 'story_id', 'day_min', 'day_max'];
    const typeRow = ['##type', 'int', 'string', 'int', 'string',
        'int', 'int', 'int', 'int',
        'bool', 'int', 'int', 'int'];
    const commentRow = ['##comment', 'ID', '名称', '需求标签', '需求描述',
        '耐心', '出现概率(%)', '民心变化', '关注度变化',
        '是否特殊', '关联剧情ID', '最早天数', '最晚天数'];
    const rows = [
        ['', 5001, '老矿工', 1, '矿洞里太暗了，需要个持久的光源', 3, 80, 1, 0, false, 0, 1, 999],
        ['', 5002, '鱼贩子', 2, '我的鱼到下午就不新鲜了，有没有保鲜的法子', 3, 70, 1, 0, false, 0, 1, 999],
        ['', 5003, '裁缝', 3, '有没有什么东西能帮我快速去除布料上的污渍', 3, 60, 1, 0, false, 0, 3, 999],
        ['', 5004, '搬运工', 4, '每天搬货把腰都累坏了，有没有省力的工具', 3, 50, 2, 0, false, 0, 5, 999],
        ['', 5005, '家庭主妇', 2, '想要个能一直保温的水壶', 4, 75, 1, 0, false, 0, 1, 999],
        ['', 5006, '夜读书生', 1, '蜡烛太暗又太费钱，有没有更好的照明', 4, 65, 1, 0, false, 0, 1, 999],
        ['', 5007, '菜农', 6, '虫子把我的菜都快吃光了！', 3, 40, 3, 1, false, 0, 10, 999],
        ['', 5008, '神秘富商', 5, '（他审视着货架上的商品，眼神锐利）', 5, 20, 0, 5, true, 0, 7, 999],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'customer');
    XLSX.writeFile(wb, path.join(DATAS, 'customer.xlsx'));
    console.log('  Created: customer.xlsx');
}

// ============================================================
// 10. avg_story.xlsx
// ============================================================
console.log('Creating avg_story.xlsx ...');
{
    const headers = ['##var', 'id', 'title', 'id_start', 'id_end', 'trigger_day', 'can_recur'];
    const typeRow = ['##type', 'int', 'string', 'int', 'int', 'int', 'bool'];
    const commentRow = ['##comment', 'ID', '标题', '起始事件ID', '结束事件ID', '触发天数', '可重复'];
    const rows = [
        ['', 6001, '相遇', 7001, 7005, 1, false],
        ['', 6002, '第一笔交易', 7006, 7009, 2, false],
        ['', 6003, '巡查来访', 7010, 7014, 3, false],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'avg_story');
    XLSX.writeFile(wb, path.join(DATAS, 'avg_story.xlsx'));
    console.log('  Created: avg_story.xlsx');
}

// ============================================================
// 11. avg_event.xlsx
// ============================================================
console.log('Creating avg_event.xlsx ...');
{
    const headers = ['##var', 'id', 'speaker', 'text', 'time_delay', 'time_duration'];
    const typeRow = ['##type', 'int', 'string', 'string', 'float', 'float'];
    const commentRow = ['##comment', 'ID', '说话人', '文本', '延迟(s)', '持续(s)'];
    const rows = [
        ['', 7001, '', '月黑风高的雨夜，莱亚从城外归来...', 0, 3],
        ['', 7002, '莱亚', '嗯？这里怎么蹲着个人...你还好吗？', 0.5, 0],
        ['', 7003, '', '一个衣衫不整、浑身湿透的少年抬起了头，金色的眼眸在暗夜中微微发光。', 0, 3],
        ['', 7004, '莱亚', '（好漂亮...）咳，跟我来吧，先去我那儿躲躲雨。', 0, 0],
        ['', 7005, '', '你跟随莱亚来到了她的工坊...', 0, 2],
        ['', 7006, '莱亚', '看看这个！我把你教我的那个发光的东西封到灯里面了！', 0, 0],
        ['', 7007, '雀', '......这也行？', 0, 0],
        ['', 7008, '莱亚', '当然行！而且我觉得这个肯定能卖钱！走，今晚就试试！', 0, 0],
        ['', 7009, '', '莱亚兴奋地拉着雀开始了第一次"营业"...', 0, 2],
        ['', 7010, '', '清晨，一阵粗暴的敲门声惊醒了两人。', 0, 2],
        ['', 7011, '巡查兵', '开门！例行检查！', 0, 0],
        ['', 7012, '莱亚', '（小声）别慌，让我来应付。你先躲到后面去。', 0, 0],
        ['', 7013, '', '莱亚打开门，堆起笑脸，熟练地塞了几枚铜币过去。', 0, 2],
        ['', 7014, '莱亚', '呼...每次都这么吓人。看来我们得赚更多钱了。', 0, 0],
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'avg_event');
    XLSX.writeFile(wb, path.join(DATAS, 'avg_event.xlsx'));
    console.log('  Created: avg_event.xlsx');
}

// ============================================================
// 12. shop_behavior.xlsx (singleton)
// ============================================================
console.log('Creating shop_behavior.xlsx ...');
{
    const headers = [
        '##var', 'notice_radius', 'notice_reset_margin', 'glance_min_seconds', 'glance_max_seconds',
        'notice_thought_bubble_chance', 'browse_thought_bubble_chance', 'unhappy_bubble_chance',
        'unhappy_bubble_seconds', 'display_bonus_per_item', 'hurry_entry_penalty', 'social_check_interval', 'greeting_radius',
        'greeting_chance', 'conversation_radius',
        'conversation_chance', 'conversation_min_seconds', 'conversation_max_seconds',
        'door_keepout_radius', 'social_cooldown_seconds', 'customer_count',
        'customer_spawn_interval', 'passerby_count', 'passerby_spawn_interval',
        'empty_display_leave_threshold', 'empty_display_bubble_seconds',
        'passerby_lane_count', 'passerby_right_to_left_chance',
        'passerby_speed_variation_percent',
        'greeting_recognition_radius', 'greeting_approach_speed_multiplier',
        'greeting_approach_max_seconds', 'social_reaction_seconds', 'social_farewell_seconds'
    ];
    const typeRow = [
        '##type', 'float', 'float', 'float', 'float', 'int', 'int', 'int', 'float', 'int', 'int', 'float', 'float', 'int',
        'float', 'float', 'float', 'float', 'float', 'int', 'float', 'float', 'float', 'float', 'float', 'float', 'int', 'float', 'int', 'float', 'int', 'float',
        'int', 'int', 'int', 'int', 'float', 'float', 'float', 'float', 'float'
    ];
    const commentRow = [
        '##comment', '店铺门口注意范围', '离开注意范围后额外移动多远才允许再次观察',
        '驻足观察最短时间(s)', '驻足观察最长时间(s)', '门口思考时出现气泡的概率(%)',
        '货架思考时出现气泡的概率(%)', '偏好不匹配后出现低落气泡的概率(%)', '低落气泡停留时间(s)',
        '每件陈列商品增加的进店吸引力(%)',
        '赶时间对进店概率的削减(%)', '社交行为检查间隔(s)', '熟人相遇触发距离',
        '熟人相遇打招呼概率(%)', 
        '普通闲聊触发距离', '普通闲聊概率(%)', '闲聊最短时间(s)', '闲聊最长时间(s)',
        '门口和柜台周围禁止社交的半径', '社交结束后的冷却时间(s)',
        '店内顾客目标数量（后续可由街区等级动态调整）', '离店后补充店内顾客的间隔(s)',
        '横穿街道的路人同时存在数量上限', '横穿路人补充间隔(s)',
        '连续看到空展台后强制离店的次数', '空展台反馈气泡显示时间(s)',
        '街道横穿路线数量', '路人从右往左通行的概率(%)',
        '路人速度随机浮动范围(+/- %)', 
        '熟人远距离认出的半径', '打招呼前短距离靠近的速度倍率', '靠近会面位置的最长时间(s)', '熟人表情每次停留时间(s)', '熟人结束时告别表情停留时间(s)'
    ];
    const dataRow = [
        '', 3.2, 2.5, 0.8, 1.4, 90, 85, 55, 0.9, 12, 60, 0.4, 1.2, 60, 1.0, 1.8, 1.35, 20, 3.0, 5.5, 1.8, 18, 24, 4.5, 24, 1.4, 3, 2.0, 6, 45, 35, 80, 4.5, 1.7, 3.5, 1.5, 1.2
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, dataRow]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'shop_behavior');
    XLSX.writeFile(wb, path.join(DATAS, 'shop_behavior.xlsx'));
    console.log('  Created: shop_behavior.xlsx');
}

// ============================================================
// 13. shop_npc_profile.xlsx
// ============================================================
console.log('Creating shop_npc_profile.xlsx ...');
{
    const headers = [
        '##var', 'id', 'profile_name', 'outdoor_only', 'pass_through', 'notice_chance', 'enter_chance', 'curiosity',
        'sociability', 'hurry', 'preferred_need_tag', 'social_group_id', 'browse_weight', 'counter_weight', 'leave_weight',
        'wander_weight', 'move_speed', 'pause_min', 'pause_max'
    ];
    const typeRow = [
        '##type', 'int', 'string', 'bool', 'bool', 'int', 'int', 'int', 'int', 'int', 'int', 'int', 'int', 'int',
        'int', 'int', 'float', 'float', 'float', 'float', 'float'
    ];
    const commentRow = [
        '##comment', '档案ID（场景角色引用）', '人物行为说明', '不会进店；若横穿街道需同时启用 pass_through',
        '从街道两侧横穿，可按注意率和进店概率改变路线',
        '经过店门时注意到店铺的概率(%)', '注意后决定进店的基础概率(%)', '好奇心(%)',
        '社交倾向(%)', '赶时间程度(%)', '偏好商品需求类别；0表示不限', '同熟人圈ID的人物视为彼此认识；0表示没有熟人圈',
        '浏览陈列柜权重', '靠近柜台权重', '离店权重', '室内闲逛权重',
        '移动速度(世界单位/s)', '普通停留最短时间(s)', '普通停留最长时间(s)'
    ];
    const rows = [
        ['', 1001, '路过街坊 A', true, true, 6, 0, 20, 60, 20, 0, 1, 0, 0, 0, 0, 0.92, 0.4, 0.8],
        ['', 1002, '路过街坊 B', true, true, 22, 0, 45, 90, 5, 0, 1, 0, 0, 0, 0, 0.92, 0.4, 0.8],
        ['', 1003, '好奇路人', false, false, 70, 62, 90, 35, 10, 1, 0, 58, 16, 12, 14, 0.78, 0.8, 2.3],
        ['', 1004, '赶路的路人', false, false, 18, 25, 35, 20, 75, 2, 0, 40, 8, 32, 20, 0.84, 0.5, 1.4],
        ['', 1005, '普通顾客', false, false, 30, 45, 55, 35, 25, 3, 2, 65, 15, 12, 8, 0.72, 0.8, 2.8],
        ['', 1006, '熟客', false, false, 45, 70, 80, 80, 10, 1, 2, 45, 25, 10, 20, 0.72, 1.0, 3.0],
        ['', 1007, '对商品好奇的路人', false, true, 42, 48, 75, 35, 25, 1, 0, 55, 15, 20, 10, 1.05, 0.8, 1.8]
    ];
    const ws = XLSX.utils.aoa_to_sheet([headers, typeRow, commentRow, ...rows]);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'shop_npc_profile');
    XLSX.writeFile(wb, path.join(DATAS, 'shop_npc_profile.xlsx'));
    console.log('  Created: shop_npc_profile.xlsx');
}

// ============================================================
// Clean up old files that are no longer needed
// ============================================================
console.log('\nCleaning up old data files...');
const oldFiles = [
    'weapon.xlsx', 'weapon_level.xlsx', 'weapon_bullet.xlsx',
    'dungeon.xlsx', 'dungeon_wave.xlsx', 'dungeon_exp.xlsx',
    'enemy.xlsx', 'enemy_level.xlsx', 'enemy_type_index.xlsx',
    'player_data.xlsx', 'player_attr_level.xlsx',
    'card.xlsx', 'chest_loop.xlsx', 'shop_drawlevel.xlsx',
    'item.xlsx', 'com_color.xlsx', 'com_drop.xlsx',
    'com_global_param.xlsx', 'com_reddot.xlsx'
];
for (const f of oldFiles) {
    const fp = path.join(DATAS, f);
    if (fs.existsSync(fp)) {
        fs.unlinkSync(fp);
        console.log(`  Deleted: ${f}`);
    }
}

console.log('\nDone! All tables created successfully.');
