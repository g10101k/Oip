export function getInitialsFromString(input: string): string {
  if (!input || input.trim().length === 0) {
    return '';
  }

  const words = input.trim().split(/\s+/);

  if (words.length === 1) {
    return words[0].charAt(0).toUpperCase();
  }

  const firstInitial = words[0].charAt(0).toUpperCase();
  const lastInitial = words[words.length - 1].charAt(0).toUpperCase();

  return firstInitial + lastInitial;
}
