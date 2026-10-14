import {
  Component,
  computed,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  signal,
  ViewChild
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SliderModule } from 'primeng/slider';
import { TranslatePipe } from '@ngx-translate/core';

/** Side of the square photo the cropper produces, in pixels. */
const OUTPUT_SIZE = 256;
/** Smallest radius of the selection, in displayed pixels. */
const MIN_RADIUS = 16;
/** Radius change for one mouse wheel step or one +/- key press. */
const ZOOM_STEP = 1.05;
/** Selection move for one arrow key press, in displayed pixels (shift multiplies it by 4). */
const MOVE_STEP = 5;
/** sin(45°) = cos(45°), places the resize handle on the circle at «16:30». */
const DIAGONAL = Math.SQRT1_2;

type DragMode = 'move' | 'resize';

/**
 * Lets the user select a round area of an image: the circle is dragged around, its size is changed with the
 * handle on the circle, the mouse wheel, the slider or the keyboard. The selected area is emitted as a square
 * PNG of 256×256 pixels.
 */
@Component({
  selector: 'user-photo-cropper',
  standalone: true,
  imports: [FormsModule, SliderModule, TranslatePipe],
  template: `
    <div class="flex flex-col gap-4">
      <div class="relative w-fit max-w-full mx-auto select-none touch-none" (wheel)="onWheel($event)">
        @if (url(); as src) {
          <img
            #image
            alt=""
            class="block max-w-full max-h-[400px]"
            draggable="false"
            [src]="src"
            (error)="loadFailed.emit()"
            (load)="onImageLoad()" />
        }
        @if (ready()) {
          <div
            class="absolute left-0 top-0 overflow-hidden pointer-events-none"
            [style.height.px]="height()"
            [style.width.px]="width()">
            <div
              class="absolute rounded-full border-2 border-dashed border-white cursor-move pointer-events-auto outline-none focus-visible:border-primary"
              id="oip-user-photo-cropper-area"
              role="slider"
              style="box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.55)"
              tabindex="0"
              [attr.aria-label]="'profileComponent.cropHint' | translate"
              [attr.aria-valuemax]="maxRadius() * 2"
              [attr.aria-valuemin]="minRadius() * 2"
              [attr.aria-valuenow]="radius() * 2"
              [style.height.px]="radius() * 2"
              [style.left.px]="centerX() - radius()"
              [style.top.px]="centerY() - radius()"
              [style.width.px]="radius() * 2"
              (keydown)="onKeyDown($event)"
              (lostpointercapture)="endDrag()"
              (pointerdown)="startDrag($event, 'move')"
              (pointermove)="onPointerMove($event)"
              (pointerup)="endDrag()"></div>
          </div>
          <div
            class="absolute w-4 h-4 -translate-x-1/2 -translate-y-1/2 rounded-full bg-white border-2 border-primary shadow cursor-nwse-resize"
            id="oip-user-photo-cropper-handle"
            [style.left.px]="centerX() + radius() * diagonal"
            [style.top.px]="centerY() + radius() * diagonal"
            (lostpointercapture)="endDrag()"
            (pointerdown)="startDrag($event, 'resize')"
            (pointermove)="onPointerMove($event)"
            (pointerup)="endDrag()"></div>
        }
      </div>
      <div class="text-sm text-muted-color text-center">{{ 'profileComponent.cropHint' | translate }}</div>
      <div class="flex items-center gap-4">
        <label class="shrink-0" for="oip-user-photo-cropper-size">{{ 'profileComponent.cropSize' | translate }}</label>
        <p-slider
          class="flex-1"
          inputId="oip-user-photo-cropper-size"
          [disabled]="!ready()"
          [max]="maxRadius()"
          [min]="minRadius()"
          [ngModel]="radius()"
          (ngModelChange)="setRadius($event)" />
      </div>
    </div>
  `
})
export class UserPhotoCropperComponent implements OnChanges, OnDestroy {
  /** Image to crop. */
  @Input({ required: true }) file!: File;

  /** Emits the selected area as a 256×256 PNG file. */
  @Output() readonly cropped = new EventEmitter<File>();

  /** Emits when the browser can't decode the file as an image. */
  @Output() readonly loadFailed = new EventEmitter<void>();

  @ViewChild('image') private imageRef?: ElementRef<HTMLImageElement>;

  readonly diagonal = DIAGONAL;
  /** The image as a data URL: the app's CSP allows data: images but not blob: ones. */
  readonly url = signal<string | null>(null);

  /** Displayed size of the image. */
  readonly width = signal(0);
  readonly height = signal(0);
  /** Selection circle in displayed pixels. */
  readonly centerX = signal(0);
  readonly centerY = signal(0);
  readonly radius = signal(0);

  readonly ready = computed(() => this.width() > 0 && this.height() > 0);
  readonly maxRadius = computed(() => Math.floor(Math.min(this.width(), this.height()) / 2));
  readonly minRadius = computed(() => Math.min(MIN_RADIUS, this.maxRadius()));

  private resizeObserver?: ResizeObserver;
  private reader?: FileReader;
  private dragMode: DragMode | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;

  ngOnChanges(): void {
    this.reader?.abort();
    this.url.set(null);
    this.width.set(0);
    this.height.set(0);
    if (!this.file) {
      return;
    }

    const reader = new FileReader();
    reader.onload = () => this.url.set(reader.result as string);
    reader.onerror = () => this.loadFailed.emit();
    reader.readAsDataURL(this.file);
    this.reader = reader;
  }

  ngOnDestroy(): void {
    this.reader?.abort();
    this.resizeObserver?.disconnect();
  }

  onImageLoad(): void {
    const image = this.imageRef!.nativeElement;
    const width = image.clientWidth;
    const height = image.clientHeight;
    this.width.set(width);
    this.height.set(height);
    this.centerX.set(width / 2);
    this.centerY.set(height / 2);
    this.radius.set(Math.max(this.minRadius(), Math.round(Math.min(width, height) * 0.4)));

    this.resizeObserver?.disconnect();
    this.resizeObserver = new ResizeObserver(() => this.onImageResize());
    this.resizeObserver.observe(image);
  }

  startDrag(event: PointerEvent, mode: DragMode): void {
    if (event.button !== 0) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    (event.target as HTMLElement).setPointerCapture(event.pointerId);
    this.dragMode = mode;

    const point = this.toImagePoint(event);
    this.dragOffsetX = point.x - this.centerX();
    this.dragOffsetY = point.y - this.centerY();
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.dragMode) {
      return;
    }

    const point = this.toImagePoint(event);
    if (this.dragMode === 'move') {
      this.moveTo(point.x - this.dragOffsetX, point.y - this.dragOffsetY);
    } else {
      this.setRadius(Math.hypot(point.x - this.centerX(), point.y - this.centerY()));
    }
  }

  endDrag(): void {
    this.dragMode = null;
  }

  onWheel(event: WheelEvent): void {
    if (!this.ready() || event.deltaY === 0) {
      return;
    }

    event.preventDefault();
    this.zoom(event.deltaY < 0 ? ZOOM_STEP : 1 / ZOOM_STEP);
  }

  onKeyDown(event: KeyboardEvent): void {
    const step = event.shiftKey ? MOVE_STEP * 4 : MOVE_STEP;
    switch (event.key) {
      case 'ArrowLeft':
        this.moveTo(this.centerX() - step, this.centerY());
        break;
      case 'ArrowRight':
        this.moveTo(this.centerX() + step, this.centerY());
        break;
      case 'ArrowUp':
        this.moveTo(this.centerX(), this.centerY() - step);
        break;
      case 'ArrowDown':
        this.moveTo(this.centerX(), this.centerY() + step);
        break;
      case '+':
      case '=':
        this.zoom(ZOOM_STEP);
        break;
      case '-':
      case '_':
        this.zoom(1 / ZOOM_STEP);
        break;
      default:
        return;
    }
    event.preventDefault();
  }

  setRadius(radius: number): void {
    this.radius.set(Math.min(this.maxRadius(), Math.max(this.minRadius(), radius)));
    this.moveTo(this.centerX(), this.centerY());
  }

  /**
   * Draws the selected area into a 256×256 canvas and emits it as a PNG file.
   */
  crop(): void {
    const image = this.imageRef?.nativeElement;
    if (!image || !this.ready()) {
      return;
    }

    const scale = this.width() / image.naturalWidth;
    const canvas = document.createElement('canvas');
    canvas.width = OUTPUT_SIZE;
    canvas.height = OUTPUT_SIZE;
    const context = canvas.getContext('2d')!;
    context.imageSmoothingQuality = 'high';
    const radius = this.radius();
    context.drawImage(
      image,
      (this.centerX() - radius) / scale,
      (this.centerY() - radius) / scale,
      (radius * 2) / scale,
      (radius * 2) / scale,
      0,
      0,
      OUTPUT_SIZE,
      OUTPUT_SIZE
    );

    // The backend accepts only image/* content, so the blob is wrapped into a typed file.
    canvas.toBlob((blob) => {
      if (blob) {
        this.cropped.emit(new File([blob], 'avatar.png', { type: 'image/png' }));
      }
    }, 'image/png');
  }

  private zoom(factor: number): void {
    this.setRadius(this.radius() * factor);
  }

  /** Moves the center, keeping the whole circle inside the image. */
  private moveTo(x: number, y: number): void {
    const radius = this.radius();
    this.centerX.set(Math.min(this.width() - radius, Math.max(radius, x)));
    this.centerY.set(Math.min(this.height() - radius, Math.max(radius, y)));
  }

  /** Keeps the selection on the same part of the image when the displayed size changes. */
  private onImageResize(): void {
    const image = this.imageRef?.nativeElement;
    const oldWidth = this.width();
    if (!image || !oldWidth || !image.clientWidth || image.clientWidth === oldWidth) {
      return;
    }

    const ratio = image.clientWidth / oldWidth;
    this.width.set(image.clientWidth);
    this.height.set(image.clientHeight);
    this.centerX.update((x) => x * ratio);
    this.centerY.update((y) => y * ratio);
    this.setRadius(this.radius() * ratio);
  }

  private toImagePoint(event: PointerEvent): { x: number; y: number } {
    const rect = this.imageRef!.nativeElement.getBoundingClientRect();
    return { x: event.clientX - rect.left, y: event.clientY - rect.top };
  }
}
