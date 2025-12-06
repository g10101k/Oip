/**
 * Convert Date to string in format 2022-02-22
 * @param date
 */
export function dateToString(date: Date): string {
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');

  return `${year}-${month}-${day}`;
}

/**
 * Convert string in format 2022-02-22 to Date
 * @param date
 */
export function stringToDate(date: string): Date {
  const dateParts = date.split('-');
  const day = parseInt(dateParts[2], 10);
  const month = parseInt(dateParts[1], 10) - 1;
  const year = parseInt(dateParts[0], 10);

  return new Date(year, month, day);
}

/**
 * Add days to date
 * @param date
 * @param days
 */
export function addDays(date: Date, days: number): Date {
  const clonedDate = structuredClone(date);
  clonedDate.setDate(clonedDate.getDate() + days);
  return clonedDate;
}

/**
 * Get today's date
 * @param date
 */
export function getTodayDate(): Date {
  const date = new Date();
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

/* Convert dateformat from DatePipe to PrimeNG*/
export function convertToPrimeNgDateFormat(dateformat: string): string {
  switch (dateformat) {
    case 'dd.MM.yyyy':
      return 'dd.mm.yy';
    case 'dd.MM.yy':
      return 'dd.mm.y';
    case 'yyyy-MM-dd':
      return 'yy-mm-dd';
    case 'dd.MMM.yyyy':
      return 'dd.M.yy';
    default:
      console.error(`Failed to convert format: ${dateformat}`);
  }
}

/**
 * Convert all properties of an object with string dates to Date objects
 * @param obj
 */
export function restoreDates(obj: any) {
  if (obj === null || typeof obj !== 'object') return obj;

  if (Array.isArray(obj)) {
    return obj.map((item) => restoreDates(item));
  }

  const result = {};
  for (const key in obj) {
    const value = obj[key];

    if (typeof value === 'string' && isIsoDate(value)) {
      result[key] = new Date(value);
    } else if (typeof value === 'object' && value !== null && !(value instanceof Date)) {
      result[key] = restoreDates(value);
    } else {
      result[key] = value;
    }
  }
  return result;
}

/**
 * Check if a string is a date in ISO format
 * @param str
 */
export function isIsoDate(str: string) {
  return /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$/.test(str);
}
