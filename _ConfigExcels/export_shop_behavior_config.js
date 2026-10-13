const fs = require('fs');
const path = require('path');
const XLSX = require('xlsx');

const dataDir = path.join(__dirname, 'Datas');
const outputDir = path.resolve(__dirname, '..', 'WelCome', 'Assets', 'Resources', 'Luban', 'Output', 'Json');

function readTable(fileName) {
    const filePath = path.join(dataDir, fileName);
    const workbook = XLSX.readFile(filePath);
    const sheet = workbook.Sheets[workbook.SheetNames[0]];
    const rows = XLSX.utils.sheet_to_json(sheet, { header: 1, raw: true, defval: '' });
    if (rows.length < 4 || rows[0][0] !== '##var' || rows[1][0] !== '##type' || rows[2][0] !== '##comment') {
        throw new Error(fileName + ' 缺少 Luban 的变量名、类型或注释行。');
    }

    const headers = rows[0];
    const types = rows[1];
    if (headers.length !== types.length) {
        throw new Error(fileName + ' 的字段名和类型数量不一致。');
    }

    const records = [];
    for (const row of rows.slice(3)) {
        if (!row.some(value => value !== '')) continue;
        const record = {};
        for (let column = 1; column < headers.length; column++) {
            const key = headers[column];
            const type = types[column];
            const value = row[column];
            if (!key || value === '' || value === null || value === undefined) {
                throw new Error(fileName + ' 第 ' + (records.length + 4) + ' 行的字段 ' + (key || column) + ' 没有填写。');
            }
            record[key] = parseValue(fileName, records.length + 4, key, type, value);
        }
        records.push(record);
    }
    return records;
}

function parseValue(fileName, rowNumber, key, type, value) {
    if (type === 'string') return String(value);
    if (type === 'bool') {
        if (value === true || value === 1 || String(value).toLowerCase() === 'true') return true;
        if (value === false || value === 0 || String(value).toLowerCase() === 'false') return false;
        throw new Error(fileName + ' 第 ' + rowNumber + ' 行的字段 ' + key + ' 不是布尔值。');
    }
    const number = Number(value);
    if (!Number.isFinite(number) || (type === 'int' && !Number.isInteger(number))) {
        throw new Error(fileName + ' 第 ' + rowNumber + ' 行的字段 ' + key + ' 不是有效的 ' + type + '。');
    }
    if (type !== 'int' && type !== 'float') {
        throw new Error(fileName + ' 的字段 ' + key + ' 使用了暂不支持的类型 ' + type + '。');
    }
    return number;
}

function validate(shopBehavior, profiles) {
    if (shopBehavior.length !== 1) throw new Error('街区行为全局参数表必须且只能有一行。');
    if (profiles.length === 0) throw new Error('街区人物行为档案表至少需要一行。');

    const settings = shopBehavior[0];
    if (settings.glance_min_seconds > settings.glance_max_seconds
        || settings.greeting_min_seconds > settings.greeting_max_seconds
        || settings.conversation_min_seconds > settings.conversation_max_seconds) {
        throw new Error('驻足、打招呼或闲聊的最短时间不能大于最长时间。');
    }
    for (const field of [
        'notice_thought_bubble_chance', 'browse_thought_bubble_chance', 'unhappy_bubble_chance',
        'passerby_right_to_left_chance', 'greeting_happy_chance'
    ]) {
        if (settings[field] < 0 || settings[field] > 100) {
            throw new Error('街区行为参数 ' + field + ' 必须在 0 到 100 之间。');
        }
    }
    if (settings.unhappy_bubble_seconds <= 0) {
        throw new Error('低落气泡停留时间必须大于 0。');
    }
    if (settings.customer_count < 0 || settings.customer_count > 40) {
        throw new Error('街区行为参数 customer_count 必须在 0 到 40 之间。');
    }
    if (settings.customer_spawn_interval <= 0) {
        throw new Error('街区行为参数 customer_spawn_interval 必须大于 0。');
    }
    if (settings.passerby_count < 0 || settings.passerby_count > 40) {
        throw new Error('街区行为参数 passerby_count 必须在 0 到 40 之间。');
    }
    if (settings.passerby_spawn_interval <= 0) {
        throw new Error('街区行为参数 passerby_spawn_interval 必须大于 0。');
    }
    if (settings.passerby_lane_count < 2 || settings.passerby_lane_count > 10) {
        throw new Error('街区行为参数 passerby_lane_count 必须在 2 到 10 之间。');
    }
    if (settings.passerby_speed_variation_percent < 0 || settings.passerby_speed_variation_percent > 80) {
        throw new Error('街区行为参数 passerby_speed_variation_percent 必须在 0 到 80 之间。');
    }
    if (settings.greeting_recognition_radius < settings.greeting_radius) {
        throw new Error('熟人识别半径不能小于最终打招呼距离。');
    }
    if (settings.greeting_approach_speed_multiplier <= 0 || settings.greeting_approach_max_seconds <= 0) {
        throw new Error('熟人靠近速度倍率和最长靠近时间必须大于 0。');
    }
    if (settings.empty_display_leave_threshold < 1 || settings.empty_display_leave_threshold > 10) {
        throw new Error('连续空展台离店次数必须在 1 到 10 之间。');
    }
    if (settings.empty_display_bubble_seconds <= 0) {
        throw new Error('空展台离店气泡显示时间必须大于 0。');
    }

    const ids = new Set();
    for (const profile of profiles) {
        if (ids.has(profile.id)) throw new Error('人物档案 ID ' + profile.id + ' 重复。');
        ids.add(profile.id);
        for (const field of ['notice_chance', 'enter_chance', 'curiosity', 'sociability', 'hurry']) {
            if (profile[field] < 0 || profile[field] > 100) {
                throw new Error('人物档案 ' + profile.id + ' 的 ' + field + ' 必须在 0 到 100 之间。');
            }
        }
        if (profile.preferred_need_tag < 0 || profile.preferred_need_tag > 6) {
            throw new Error('人物档案 ' + profile.id + ' 的 preferred_need_tag 必须在 0 到 6 之间。');
        }
        if (profile.outdoor_only && !profile.pass_through) {
            throw new Error('仅在店外活动的人物档案必须启用 pass_through 街道路线。');
        }
        for (const field of ['browse_weight', 'counter_weight', 'leave_weight', 'wander_weight']) {
            if (profile[field] < 0) throw new Error('人物档案 ' + profile.id + ' 的 ' + field + ' 不能小于 0。');
        }
        if (profile.move_speed <= 0 || profile.pause_min < 0 || profile.pause_min > profile.pause_max) {
            throw new Error('人物档案 ' + profile.id + ' 的移动速度或停留时间设置无效。');
        }
        if (profile.pass_through && profile.outdoor_only && profile.enter_chance > 0) {
            throw new Error('仅在室外活动的横穿路人不能配置进店概率。');
        }
    }
    if (settings.customer_count > 0 && !profiles.some(profile => !profile.pass_through && !profile.outdoor_only)) {
        throw new Error('店内顾客数量大于 0 时，至少需要一个可进店的普通顾客档案。');
    }
    if (settings.passerby_count > 0 && !profiles.some(profile => profile.pass_through)) {
        throw new Error('街道横穿路人数量大于 0，但人物档案表没有配置横穿路人。');
    }
}

function writeJson(fileName, value) {
    fs.writeFileSync(path.join(outputDir, fileName), JSON.stringify(value, null, 2) + '\n', 'utf8');
}

try {
    const shopBehavior = readTable('shop_behavior.xlsx');
    const profiles = readTable('shop_npc_profile.xlsx');
    validate(shopBehavior, profiles);
    writeJson('tbshopbehavior.json', shopBehavior);
    writeJson('tbshopnpcprofile.json', profiles);
    console.log('已导出街区行为参数 1 行、人物档案 ' + profiles.length + ' 行。');
    console.log('返回 Unity 后等待资源导入，再进入 Play 模式即可使用。');
} catch (error) {
    console.error('导出失败：' + error.message);
    process.exitCode = 1;
}
