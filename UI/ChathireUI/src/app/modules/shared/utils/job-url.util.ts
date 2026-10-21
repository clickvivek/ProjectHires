/**
 * Generates an obfuscated unique share token containing the numeric job ID
 * embedded with random noise characters and hyphenated segments to deter web scrapers.
 * Format: XXXX-XXXX-XXXX-R<JobID>R-XXXXXX-XXXX-XXXX-XXXX-XXXX
 */
const cachedJobShareTokens: { [key: string]: string } = {};

export function generateObfuscatedJobId(jobId: number | string): string {
  if (!jobId) return '';
  const key = String(jobId);
  if (cachedJobShareTokens[key]) {
    return cachedJobShareTokens[key];
  }

  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';

  // 3 segments of 4 random characters
  for (let i = 0; i < 3; i++) {
    for (let j = 0; j < 4; j++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    result += '-';
  }

  // 4th segment: 1 random char + real jobId + 1 random char + '-'
  const prefixChar = chars.charAt(Math.floor(Math.random() * chars.length));
  const suffixChar = chars.charAt(Math.floor(Math.random() * chars.length));
  result += prefixChar + key + suffixChar + '-';

  // 5th segment: 6 random characters
  for (let i = 0; i < 6; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }

  // Next 4 segments of 4 random characters
  for (let i = 0; i < 4; i++) {
    result += '-';
    for (let j = 0; j < 4; j++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
  }

  cachedJobShareTokens[key] = result;
  return result;
}

/**
 * Parses either a plain numeric ID, a legacy base64 encoded token,
 * or an obfuscated random-segmented token back into the raw numeric job ID.
 */
export function parseObfuscatedJobId(rawId: any): string | null {
  if (!rawId) return null;
  let strId = String(rawId).trim();

  try {
    strId = decodeURIComponent(strId);
  } catch (e) {}

  // 1. Plain numeric ID (backward compatibility)
  if (/^\d+$/.test(strId)) {
    return strId;
  }

  // 2. Direct 'chathire_1040'
  if (strId.startsWith('chathire_')) {
    const extracted = strId.replace('chathire_', '');
    if (/^\d+$/.test(extracted)) return extracted;
  }

  // 3. Base64 decoded (e.g. 'Y2hhdGhpcmVfMTA0MA==' -> 'chathire_1040')
  try {
    const decoded = atob(strId);
    if (decoded.startsWith('chathire_')) {
      const extracted = decoded.replace('chathire_', '');
      if (/^\d+$/.test(extracted)) return extracted;
    }
    if (/^\d+$/.test(decoded)) {
      return decoded;
    }
  } catch (e) {
    // not valid base64
  }

  // 4. Obfuscated hyphenated format (e.g. 809S-PefT-8B3F-D275I-oDEY2n-caFj-dewK-6D3U-b8pe)
  if (strId.includes('-')) {
    const parts = strId.split('-');
    // In our format, parts[3] is <prefixChar><jobId><suffixChar>
    if (parts.length >= 4 && parts[3].length > 2) {
      const inner = parts[3].substring(1, parts[3].length - 1);
      if (/^\d+$/.test(inner)) {
        return inner;
      }
    }
    // Fallback: search any part containing numeric sequence
    for (const part of parts) {
      const match = part.match(/\d+/);
      if (match && match[0].length >= 1) {
        return match[0];
      }
    }
  }

  return null;
}
