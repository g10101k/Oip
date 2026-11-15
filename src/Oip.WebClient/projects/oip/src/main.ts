// The startup screen stays at least this long after the page opens, so the logo animation can play.
const minStartupScreenMs = 3500;

// Matches the fade-out transition in startup.css; the screen is removed after it even if no transition runs.
const startupScreenFadeMs = 500;

const startupScreenShown = new Promise<void>((resolve) =>
  setTimeout(resolve, Math.max(0, minStartupScreenMs - performance.now()))
);

/**
 * Gives Angular a new empty app-root in front of the one holding the startup screen.
 * Angular mounts into the first app-root, so the screen is not touched; moving it
 * in the DOM would restart its logo animation.
 */
function detachStartupScreen(): HTMLElement | null {
  const host = document.querySelector<HTMLElement>('app-root');
  if (!host?.querySelector('.oip-startup')) {
    return null;
  }
  host.before(document.createElement('app-root'));
  host.style.display = 'contents';
  return host;
}

/** Fades the startup screen out over the rendered application, then removes it. */
function hideStartupScreen(host: HTMLElement | null): void {
  const screen = host?.querySelector<HTMLElement>('.oip-startup');
  if (!host || !screen) {
    return;
  }
  const remove = () => host.remove();
  screen.addEventListener('transitionend', remove, { once: true });
  setTimeout(remove, startupScreenFadeMs + 100);
  screen.classList.add('oip-startup--leaving');
}

Promise.all([import('./bootstrap'), startupScreenShown])
  .then(async ([app]) => {
    const screen = detachStartupScreen();
    await app.bootstrap();
    hideStartupScreen(screen);
  })
  .catch((err) => console.error(err));
