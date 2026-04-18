# Управление темами

## Контракт темы

Тема описывается интерфейсом `AppThemePreset`.

```ts
export interface AppThemePreset {
    id: string;
    label?: string;
    preset: Preset;
    primaryColors?: Record<string, PaletteDesignToken | undefined>;
    surfaceColors?: Record<string, PaletteDesignToken | undefined>;
}
```

Поля:

| Поле            | Описание                                                           |
|-----------------|--------------------------------------------------------------------|
| `id`            | Уникальный идентификатор темы. Сохраняется в `layoutConfig.preset` |
| `label`         | Название темы для отображения в UI                                 |
| `preset`        | PrimeNG preset, обычно созданный через `definePreset(...)`         |
| `primaryColors` | Доступные accent/primary цвета в палитре настроек                  |
| `surfaceColors` | Доступные цвета фона в палитре настроек                            |

Если `primaryColors` задан, компонент настроек показывает только перечисленные primary-цвета. Встроенный список цветов в
этом случае не добавляется.

Если `surfaceColors` задан, компонент настроек показывает только перечисленные surface-цвета. Встроенный список цветов
фона в этом случае не добавляется.

## Подключение тем

Темы подключаются через `provideAppThemes`.

```ts
provideAppThemes(appTheme, {mode: 'replaceDefaults'})
```

Компонент `AppConfiguratorComponent` читает эти токены и строит список доступных тем.

## Режимы подключения

Доступны два режима.

| Режим               | Поведение                                                                                                                                 |
|---------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `mergeWithDefaults` | Пользовательские темы добавляются к стандартным `Aura`, `Lara`, `Nora`. Если `id` совпадает, стандартная тема заменяется пользовательской |
| `replaceDefaults`   | Стандартные темы не добавляются. В UI будут только темы, переданные в `provideAppThemes`                                                  |

По умолчанию используется `mergeWithDefaults`.

Для явного выбора режима можно использовать:

```ts
provideAppThemes(appTheme, {mode: 'replaceDefaults'})
```

Также доступны helper-функции:

```ts
mergeWithDefaults(appTheme)
replaceDefaults(appTheme)
```

## Конфигурация темы приложения

Текущая конфигурация темы приложения вынесена в файл:

```ts
src/Oip.WebClient/projects/oip/src/app.theme.ts
```

Пример текущей структуры:

```ts
import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';
import { primitive } from '@primeng/themes/aura/base';
import { AppThemePreset } from 'oip-common';

export const appTheme: AppThemePreset[] = [
    {
        id: 'Oip',
        label: 'Oip',
        preset: definePreset(Aura, {
            components: {
                toolbar: {
                    colorScheme: {
                        light: {
                            root: {
                                borderRadius: '0.7rem'
                            }
                        },
                        dark: {
                            root: {
                                borderRadius: '0.7rem'
                            }
                        }
                    }
                }
            }
        }),
        primaryColors: {
            rose: primitive['red']
        },
        surfaceColors: {
            zinc: {
                0: '#ffffff',
                50: '#fafafa',
                100: '#f4f4f5',
                200: '#e4e4e7',
                300: '#d4d4d8',
                400: '#a1a1aa',
                500: '#71717a',
                600: '#52525b',
                700: '#3f3f46',
                800: '#27272a',
                900: '#18181b',
                950: '#09090b'
            }
        }
    }
];
```

В этом примере:

- в списке тем будет один пресет `Oip`;
- в primary-палитре будет один цвет с именем `rose`, но фактически он использует палитру `primitive['red']`;
- в surface-палитре будет один цвет фона `zinc`;
- остальные primary и surface цвета в компоненте настроек показаны не будут.

## Primary-цвета

`primaryColors` управляет палитрой акцента в компоненте настроек.

Ключ объекта используется как имя цвета:

```json
primaryColors: {
   green: primitive['green'],
   rose: primitive['rose']
}
```

Значение должно быть PrimeNG palette token:

```json
{
  50: '#...',
  100: '#...',
  200: '#...',
  300: '#...',
  400: '#...',
  500: '#...',
  600: '#...',
  700: '#...',
  800: '#...',
  900: '#...',
  950: '#...'
}
```

Для темной темы особенно важны оттенки `200`, `300` и `400`, потому что текущая схема PrimeNG использует их для
`primary.color`, `hoverColor` и `activeColor` в dark mode.

Если нужно добавить все стандартные primary-цвета Aura, можно собрать объект из `primitive`:

```ts
const primaryColors = {
    emerald: primitive['emerald'],
    green: primitive['green'],
    lime: primitive['lime'],
    orange: primitive['orange'],
    amber: primitive['amber'],
    yellow: primitive['yellow'],
    teal: primitive['teal'],
    cyan: primitive['cyan'],
    sky: primitive['sky'],
    blue: primitive['blue'],
    indigo: primitive['indigo'],
    violet: primitive['violet'],
    purple: primitive['purple'],
    fuchsia: primitive['fuchsia'],
    pink: primitive['pink'],
    rose: primitive['rose']
};
```
Если `primaryColors` не задан, компонент настроек использует встроенный fallback-список.


## Цвета фона

`surfaceColors` управляет палитрой цвета фона в компоненте настроек.

Пример:

```json
surfaceColors: {
   slate: {
      0: '#ffffff',
      ...
      primitive['slate']
   },
   zinc: {
      0: '#ffffff',
      ...
      primitive['zinc']
   }
}
```

Для surface-палитр рекомендуется задавать ключ `0: '#ffffff'`. PrimeNG использует `surface.0` для светлых областей
интерфейса.

Если `surfaceColors` не задан, компонент настроек использует встроенный fallback-список.

Если `surfaceColors` задан, показываются только цвета из него.

