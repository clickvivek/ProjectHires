export const picUrl = 'https://hiresblob.blob.core.windows.net/profilepic/'; 
export const defaultProfilePic = 'assets/images/profile-placehoder-img.png'; 

export const publicProfileUrlPrefix = 'www.chathire.com/profile/';

export function getFullPublicProfileUrl(slugOrUsername?: string): string {
  if (!slugOrUsername) return '';
  const cleanSlug = String(slugOrUsername).trim()
    .replace(/^https?:\/\/[^\/]+\/(#\/)?(profile\/)?/i, '')
    .replace(/^www\.chathire\.com\/(#\/)?(profile\/)?/i, '')
    .replace(/^public-profile\//i, '')
    .replace(/^profile\//i, '');
  if (!cleanSlug) return '';
  const origin = (typeof window !== 'undefined' && window.location?.origin)
    ? window.location.origin
    : 'https://www.chathire.com';
  return `${origin}/#/profile/${cleanSlug}`;
}

export const profileInitialCountSummary = [
    { candidateProfileMappingStatusId: 1, candidateProfileMappingStatusName: "New", count: 0 },
    { candidateProfileMappingStatusId: 2, candidateProfileMappingStatusName: "On Hold", count: 0 },
    { candidateProfileMappingStatusId: 3, candidateProfileMappingStatusName: "Shortlisted", count: 0 },
    { candidateProfileMappingStatusId: 4, candidateProfileMappingStatusName: "No Response", count: 0 },
    { candidateProfileMappingStatusId: 6, candidateProfileMappingStatusName: "Rejected", count: 0 },
    { candidateProfileMappingStatusId: 7, candidateProfileMappingStatusName: "Submitted", count: 0 },
    { candidateProfileMappingStatusId: 5, candidateProfileMappingStatusName: "Interview", count: 0 },
]

export const defaultPostJobVisas = [
    {
        "id": 4,
        "visaId": 4,
        "name": "GC",
        "description": "GreenCard Holder",
        "updatedBy": null
    },
    {
        "id": 5,
        "visaId": 5,
        "name": "USC",
        "description": "US Citizen",
        "updatedBy": null
    }
]

export const defaultPostJobPositionTypes = [
    {
        "id": 7,
        "jobTypeId": 7,
        "description": "C2C - Contract"
    },
    {
        "id": 8,
        "jobTypeId": 8,
        "description": "W2 - Contract"
    },
    {
        "id": 11,
        "jobTypeId": 11,
        "description": "W2 - Full time"
    }
]

export const US_STATE_MAP: { [key: string]: string } = {
  'alabama': 'AL', 'alaska': 'AK', 'arizona': 'AZ', 'arkansas': 'AR', 'california': 'CA',
  'colorado': 'CO', 'connecticut': 'CT', 'delaware': 'DE', 'florida': 'FL', 'georgia': 'GA',
  'hawaii': 'HI', 'idaho': 'ID', 'illinois': 'IL', 'indiana': 'IN', 'iowa': 'IA',
  'kansas': 'KS', 'kentucky': 'KY', 'louisiana': 'LA', 'maine': 'ME', 'maryland': 'MD',
  'massachusetts': 'MA', 'michigan': 'MI', 'minnesota': 'MN', 'mississippi': 'MS', 'missouri': 'MO',
  'montana': 'MT', 'nebraska': 'NE', 'nevada': 'NV', 'new hampshire': 'NH', 'new jersey': 'NJ',
  'new mexico': 'NM', 'new york': 'NY', 'north carolina': 'NC', 'north dakota': 'ND', 'ohio': 'OH',
  'oklahoma': 'OK', 'oregon': 'OR', 'pennsylvania': 'PA', 'rhode island': 'RI', 'south carolina': 'SC',
  'south dakota': 'SD', 'tennessee': 'TN', 'texas': 'TX', 'utah': 'UT', 'vermont': 'VT',
  'virginia': 'VA', 'washington': 'WA', 'west virginia': 'WV', 'wisconsin': 'WI', 'wyoming': 'WY',
  'district of columbia': 'DC'
};

export const US_STATE_CODES = new Set(Object.values(US_STATE_MAP));

export function formatRelocation(data: any): string {
  if (!data) return 'Open';

  let items: any[] = [];
  if (Array.isArray(data)) {
    items = data;
  } else if (data.candidatePrefLocations && Array.isArray(data.candidatePrefLocations)) {
    items = data.candidatePrefLocations;
  }

  if (!items || items.length === 0) {
    if (data?.anyLocation) return 'Any Location';
    if (data?.remoteOnly) return 'Remote';
    return 'Open';
  }

  interface LocationResult {
    isCity: boolean;
    text: string;
  }

  const results: LocationResult[] = [];
  const seenStates = new Set<string>();

  for (const item of items) {
    let rawCity = '';
    let stateCode = '';
    let stateName = '';

    if (typeof item === 'string') {
      rawCity = item.trim();
    } else if (item && typeof item === 'object') {
      rawCity = (item.cityName || '').trim();
      stateCode = (item.stateCode || '').trim();
      stateName = (item.stateName || '').trim();
    }

    let cityPart = rawCity;
    let extractedState = '';
    if (rawCity.includes('-')) {
      const parts = rawCity.split('-');
      cityPart = (parts[0] || '').trim();
      extractedState = (parts[1] || '').trim();
    } else if (rawCity.includes(',')) {
      const parts = rawCity.split(',');
      cityPart = (parts[0] || '').trim();
      extractedState = (parts[1] || '').trim();
    }

    const rawState = (stateCode || extractedState || stateName).trim();
    let resolvedStateCode = '';
    if (rawState) {
      if (US_STATE_CODES.has(rawState.toUpperCase())) {
        resolvedStateCode = rawState.toUpperCase();
      } else if (US_STATE_MAP[rawState.toLowerCase()]) {
        resolvedStateCode = US_STATE_MAP[rawState.toLowerCase()];
      }
    }

    const cityLower = cityPart.toLowerCase();
    const cityUpper = cityPart.toUpperCase();
    const isCityActuallyStateName = !!US_STATE_MAP[cityLower];
    const isCityActuallyStateCode = US_STATE_CODES.has(cityUpper);

    if (isCityActuallyStateName || isCityActuallyStateCode) {
      const code = isCityActuallyStateName ? US_STATE_MAP[cityLower] : cityUpper;
      if (!seenStates.has(code)) {
        seenStates.add(code);
        results.push({ isCity: false, text: code });
      }
    } else if (cityPart) {
      let formattedCity = cityPart;
      const sCode = resolvedStateCode || (US_STATE_MAP[rawState.toLowerCase()] || rawState);
      if (sCode) {
        formattedCity = `${cityPart}, ${sCode}`;
      }
      results.push({ isCity: true, text: formattedCity });
    } else if (resolvedStateCode) {
      if (!seenStates.has(resolvedStateCode)) {
        seenStates.add(resolvedStateCode);
        results.push({ isCity: false, text: resolvedStateCode });
      }
    } else if (rawState) {
      results.push({ isCity: false, text: rawState });
    }
  }

  if (results.length === 0) {
    if (data?.anyLocation) return 'Any Location';
    if (data?.remoteOnly) return 'Remote';
    return 'Open';
  }

  const hasAnyCity = results.some(r => r.isCity);
  if (!hasAnyCity) {
    return results.map(r => r.text).join(', ');
  } else {
    return results.map(r => r.text).join('; ');
  }
}