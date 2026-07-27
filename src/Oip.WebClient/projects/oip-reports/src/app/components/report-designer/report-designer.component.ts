import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { BaseModuleComponent, ContentType } from 'oip-common';

type BandType =
  'ReportHeader'
  | 'PageHeader'
  | 'GroupHeader'
  | 'Detail'
  | 'GroupFooter'
  | 'PageFooter'
  | 'ReportFooter'
  | 'SubReport';
type ElementType = 'Text' | 'Value' | 'Line' | 'Rectangle' | 'Image';

interface Layout {
  x: number;
  y: number;
  width: number;
  height: number;
  zIndex: number;
}

interface ElementDefinition {
  id: string;
  type: ElementType;
  label?: string;
  textTemplate?: string;
  valuePath?: string;
  sourceUrl?: string;
  styleId?: string;
  layout: Layout;
}

interface BandDefinition {
  id: string;
  type: BandType;
  title?: string;
  visible: boolean;
  displayCondition?: string;
  height: number;
  pageBreak?: string;
  repeatOnEachPage?: boolean;
  grouping?: { expression: string; sortDirection: string; summaries: any[] };
  elements: ElementDefinition[];
}

interface StyleDefinition {
  id: string;
  cssClass?: string;
  properties: Record<string, string>;
}

interface ReportDefinition {
  id: string;
  name: string;
  description?: string;
  currentVersion?: number;
  bands: BandDefinition[];
  styles: StyleDefinition[];
  page: {
    width: number;
    height: number;
    unit: string;
    margins: { left: number; right: number; top: number; bottom: number }
  };
}

interface SchemaField {
  path: string;
  label: string;
  type: string;
}

interface Schema {
  dataSourceKey: string;
  fields: SchemaField[];
}

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, Button, InputText, InputNumber, Select],
  template: `
    @if (isContent && draft) {
      <div class="designer-shell">
        <header class="designer-toolbar">
          <div><p class="eyebrow">REPORT DESIGNER</p>
            <h3>{{ draft.name }}</h3><span
              [class.text-amber-600]="dirty">{{ dirty ? 'Есть несохранённые изменения' : 'Все изменения сохранены' }}</span>
          </div>
          <div class="toolbar-actions">
            <p-button icon="pi pi-undo" severity="secondary" [disabled]="!undoStack.length" (onClick)="undo()"/>
            <p-button icon="pi pi-replay" severity="secondary" [disabled]="!redoStack.length" (onClick)="redo()"/>
            <p-button label="Отменить изменения" severity="secondary" (onClick)="discard()"/>
            <p-button label="Сохранить" icon="pi pi-save" [loading]="saving" (onClick)="save()"/>
            <p-button label="Удалить" icon="pi pi-trash" severity="danger" [disabled]="saving" (onClick)="remove()"/>
          </div>
        </header>

        <div class="designer-grid">
          <aside class="designer-panel">
            <h4>Band’ы</h4>
            <div class="band-list">
              @for (band of draft.bands; track band.id) {
                <button class="band-item" [class.active]="selectedBand?.id === band.id"
                        (click)="selectBand(band)">{{ band.type }}
                </button>
              }
            </div>
            <p-select class="w-full mt-3" [options]="bandTypes" [(ngModel)]="newBandType"/>
            <p-button class="mt-2" label="Добавить band" severity="secondary" (onClick)="addBand()"/>
            <hr/>
            <h4>Элементы</h4>
            <div class="toolbox">
              @for (item of elementTypes; track item.value) {
                <button (click)="addElement(item.value)"><i [class]="item.icon"></i>{{ item.label }}</button>
              }
            </div>
          </aside>

          <main class="workspace">
            <div class="zoom-controls">
              <button (click)="setZoom(zoom - .1)">−</button>
              <span>{{ zoom | number:'1.0-0' }}×</span>
              <button (click)="setZoom(zoom + .1)">+</button>
            </div>
            <div class="canvas-viewport">
              <div class="report-canvas" [style.width.px]="pageWidth * pixelScale"
                   [style.minHeight.px]="pageHeight * pixelScale" [style.transform]="'scale(' + zoom + ')'">
                @for (band of draft.bands; track band.id) {
                  <section class="canvas-band" [class.selected]="selectedBand?.id === band.id"
                           [style.height.px]="band.height * pixelScale"
                           (click)="selectBand(band); $event.stopPropagation()">
                    <span class="band-label">{{ band.type }}</span>
                    @for (element of band.elements; track element.id) {
                      <div class="canvas-element" [class.selected]="selectedElement?.id === element.id"
                           [ngStyle]="elementStyle(element)" (pointerdown)="startMove($event, band, element)"
                           (click)="$event.stopPropagation(); selectElement(band, element)">
                        @switch (element.type) {
                          @case ('Line') {
                            <span class="line-preview"></span>
                          }
                          @case ('Rectangle') {
                            <span class="rectangle-preview"></span>
                          }
                          @case ('Image') {
                            <img [src]="element.sourceUrl || 'https://placehold.co/200x100?text=Image'" alt=""/>
                          }
                          @case ('Value') {
                            <span>{{ element.label || fieldLabel(element.valuePath) || 'Поле' }}</span>
                          }
                          @default {
                            <span>{{ element.textTemplate || 'Текст' }}</span>
                          }
                        }
                        @if (selectedElement?.id === element.id) {
                          <span class="resize-handle" (pointerdown)="startResize($event, band, element)"></span>
                        }
                      </div>
                    }
                  </section>
                }
              </div>
            </div>
          </main>

          <aside class="designer-panel properties">
            @if (selectedElement; as element) {
              <h4>Свойства элемента</h4>
              <label>Тип <input pInputText [ngModel]="element.type" disabled/></label>
              @if (element.type === 'Text') {
                <label>Текст <input pInputText [(ngModel)]="element.textTemplate" (ngModelChange)="changed()"/></label>
              }
              @if (element.type === 'Value') {
                <label>Поле
                  <p-select [options]="fieldOptions" optionLabel="label" optionValue="value"
                            [(ngModel)]="element.valuePath" (ngModelChange)="changed()"/>
                </label>
              }
              @if (element.type === 'Image') {
                <label>URL изображения <input pInputText [(ngModel)]="element.sourceUrl"
                                              (ngModelChange)="changed()"/></label>
              }
              <div class="property-grid"><label>X
                <p-inputnumber [(ngModel)]="element.layout.x" (ngModelChange)="changed()"/>
              </label><label>Y
                <p-inputnumber [(ngModel)]="element.layout.y" (ngModelChange)="changed()"/>
              </label><label>Ширина
                <p-inputnumber [(ngModel)]="element.layout.width" (ngModelChange)="changed()"/>
              </label><label>Высота
                <p-inputnumber [(ngModel)]="element.layout.height" (ngModelChange)="changed()"/>
              </label></div>
              <label>Цвет <input type="color" [ngModel]="styleProperty(element, 'color', '#17212b')"
                                 (ngModelChange)="setStyle(element, 'color', $event)"/></label>
              <label>Фон <input type="color" [ngModel]="styleProperty(element, 'background-color', '#ffffff')"
                                (ngModelChange)="setStyle(element, 'background-color', $event)"/></label>
              <label>Размер шрифта
                <p-inputnumber [ngModel]="styleNumber(element, 'font-size', 12)"
                               (ngModelChange)="setStyle(element, 'font-size', $event + 'pt')"/>
              </label>
              <p-button label="На передний план" severity="secondary" (onClick)="bringToFront(element)"/>
              <p-button label="Удалить элемент" severity="danger" (onClick)="deleteElement()"/>
            } @else if (selectedBand; as band) {
              <h4>Свойства band’а</h4>
              <label>Тип
                <p-select [options]="bandTypes" [(ngModel)]="band.type" (ngModelChange)="changed()"/>
              </label>
              <label>Высота
                <p-inputnumber [(ngModel)]="band.height" (ngModelChange)="changed()"/>
              </label>
              <label>Условие <input pInputText [(ngModel)]="band.displayCondition" placeholder="parameter:name == value"
                                    (ngModelChange)="changed()"/></label>
              <label><input type="checkbox" [(ngModel)]="band.visible" (ngModelChange)="changed()"/> Видим</label>
              <label><input type="checkbox" [(ngModel)]="band.repeatOnEachPage" (ngModelChange)="changed()"/> Повторять
                на странице</label>
              @if (isGroupBand(band)) {
                <label>Группировка
                  <p-select [options]="fieldOptions" optionLabel="label" optionValue="value"
                            [(ngModel)]="band.grouping!.expression" (ngModelChange)="changed()"/>
                </label>
                <label>Сортировка
                  <p-select [options]="sortDirections" [(ngModel)]="band.grouping!.sortDirection"
                            (ngModelChange)="changed()"/>
                </label>
              }
              <p-button label="Удалить band" severity="danger" (onClick)="deleteBand()"/>
            } @else {
              <p class="text-surface-500">Выберите band или элемент на холсте.</p>
            }
          </aside>
        </div>
      </div>
    }
  `,
  styles: [`
    .designer-shell {
      display: flex;
      flex-direction: column;
      gap: 1rem
    }

    .designer-toolbar {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      align-items: center;
      padding: 1rem 1.25rem;
      background: #fff;
      border: 1px solid var(--p-content-border-color);
      border-radius: 1rem
    }

    .designer-toolbar h3 {
      margin: .1rem 0
    }

    .eyebrow {
      margin: 0;
      color: #0f766e;
      font-size: .7rem;
      font-weight: 700;
      letter-spacing: .14em
    }

    .toolbar-actions {
      display: flex;
      gap: .5rem;
      flex-wrap: wrap
    }

    .designer-grid {
      display: grid;
      grid-template-columns:240px minmax(0, 1fr) 290px;
      gap: 1rem;
      min-height: 720px
    }

    .designer-panel {
      background: #fff;
      border: 1px solid var(--p-content-border-color);
      border-radius: 1rem;
      padding: 1rem;
      overflow: auto
    }

    .designer-panel h4 {
      margin-top: 0
    }

    .band-list, .toolbox {
      display: flex;
      flex-direction: column;
      gap: .4rem
    }

    .band-item, .toolbox button {
      border: 1px solid #e2e8f0;
      background: #fff;
      padding: .55rem;
      text-align: left;
      border-radius: .45rem;
      cursor: pointer
    }

    .band-item.active, .band-item:hover, .toolbox button:hover {
      border-color: #14b8a6;
      background: #f0fdfa
    }

    .workspace {
      position: relative;
      overflow: auto;
      background: #e2e8f0;
      border-radius: 1rem;
      padding: 3rem 2rem
    }

    .zoom-controls {
      position: sticky;
      top: 0;
      z-index: 3;
      display: flex;
      width: max-content;
      gap: .5rem;
      align-items: center;
      background: #fff;
      padding: .4rem .6rem;
      border-radius: .5rem;
      box-shadow: 0 1px 4px #94a3b8
    }

    .canvas-viewport {
      padding: 2rem 0;
      transform-origin: top center
    }

    .report-canvas {
      transform-origin: top left;
      background-color: #fff;
      background-image: linear-gradient(#e2e8f0 1px, transparent 1px), linear-gradient(90deg, #e2e8f0 1px, transparent 1px);
      background-size: 10px 10px;
      box-shadow: 0 8px 30px #64748b;
      margin: auto
    }

    .canvas-band {
      position: relative;
      border-bottom: 1px dashed #94a3b8;
      min-width: 100%;
      cursor: pointer
    }

    .canvas-band.selected {
      box-shadow: inset 0 0 0 2px #14b8a6
    }

    .band-label {
      position: absolute;
      right: 2px;
      top: 2px;
      color: #64748b;
      font-size: 10px;
      pointer-events: none
    }

    .canvas-element {
      position: absolute;
      border: 1px solid transparent;
      cursor: move;
      overflow: hidden;
      padding: 2px;
      font-size: 12px
    }

    .canvas-element.selected {
      border: 2px solid #0d9488
    }

    .canvas-element img {
      width: 100%;
      height: 100%;
      object-fit: contain
    }

    .line-preview {
      display: block;
      border-top: 1px solid #17212b;
      margin-top: 50%
    }

    .rectangle-preview {
      display: block;
      width: 100%;
      height: 100%;
      border: 1px solid #17212b
    }

    .resize-handle {
      position: absolute;
      right: -5px;
      bottom: -5px;
      width: 10px;
      height: 10px;
      background: #0d9488;
      cursor: nwse-resize
    }

    .properties label {
      display: flex;
      flex-direction: column;
      gap: .35rem;
      margin-bottom: .7rem;
      font-size: .85rem
    }

    .properties label:has(input[type=checkbox]) {
      display: block
    }

    .property-grid {
      display: grid;
      grid-template-columns:1fr 1fr;
      gap: .5rem
    }

    .properties p-button {
      display: block;
      margin-top: .6rem
    }

    @media(max-width: 1100px) {
      .designer-grid {
        grid-template-columns:1fr
      }
      .designer-panel {
        max-height: 340px
      }
      .workspace {
        min-height: 650px
      }
    }
  `]
})
export class ReportDesignerComponent extends BaseModuleComponent<object, object> implements OnInit, OnDestroy {
  protected draft: ReportDefinition | null = null;
  private source: ReportDefinition | null = null;
  protected schemas: Schema[] = [];
  protected selectedBand: BandDefinition | null = null;
  protected selectedElement: ElementDefinition | null = null;
  protected undoStack: string[] = [];
  protected redoStack: string[] = [];
  protected dirty = false;
  protected saving = false;
  protected zoom = 1;
  protected newBandType: BandType = 'Detail';
  protected readonly pixelScale = 3.78;
  protected readonly bandTypes = ['ReportHeader', 'PageHeader', 'GroupHeader', 'Detail', 'GroupFooter', 'PageFooter', 'ReportFooter', 'SubReport'];
  protected readonly sortDirections = ['Ascending', 'Descending'];
  protected readonly elementTypes: { label: string; value: ElementType; icon: string }[] = [
    {label: 'Текст', value: 'Text', icon: 'pi pi-align-left'}, {
      label: 'Поле',
      value: 'Value',
      icon: 'pi pi-database'
    }, {label: 'Линия', value: 'Line', icon: 'pi pi-minus'}, {
      label: 'Прямоугольник',
      value: 'Rectangle',
      icon: 'pi pi-stop'
    }, {label: 'Изображение', value: 'Image', icon: 'pi pi-image'}
  ];
  private activePointer?: {
    band: BandDefinition;
    element: ElementDefinition;
    startX: number;
    startY: number;
    layout: Layout;
    resize: boolean
  };

  protected get pageWidth(): number {
    return this.draft ? this.draft.page.width - this.draft.page.margins.left - this.draft.page.margins.right : 180;
  }

  protected get pageHeight(): number {
    return this.draft ? this.draft.page.height - this.draft.page.margins.top - this.draft.page.margins.bottom : 267;
  }

  protected get fieldOptions(): { label: string; value: string }[] {
    return this.schemas.reduce((result: {
      label: string;
      value: string
    }[], schema) => result.concat(schema.fields.map(field => ({
      label: `${field.label} (${field.type})`,
      value: field.path
    }))), []);
  }

  override async ngOnInit(): Promise<void> {
    await super.ngOnInit();
    await this.load();
    window.addEventListener('pointermove', this.onPointerMove);
    window.addEventListener('pointerup', this.onPointerUp);
  }

  override ngOnDestroy(): void {
    window.removeEventListener('pointermove', this.onPointerMove);
    window.removeEventListener('pointerup', this.onPointerUp);
    super.ngOnDestroy();
  }

  protected async load(): Promise<void> {
    const reportId = this.getReportId();
    if (!reportId) return;
    try {
      const definition = await this.httpClient.request<ReportDefinition>({
        path: '/api/report/get-report-by-id',
        method: 'GET',
        query: {id: reportId},
        secure: true,
        format: 'json'
      });
      this.source = structuredClone(definition);
      this.draft = structuredClone(definition);
      this.selectedBand = this.draft.bands[0] ?? null;
      this.schemas = await this.httpClient.request<Schema[]>({
        path: '/api/report/get-report-data-source-schema-by-report-id',
        method: 'GET',
        query: {id: reportId},
        secure: true,
        format: 'json'
      });
    } catch (error) {
      this.msgService.error(error);
    }
  }

  protected selectBand(band: BandDefinition): void {
    this.selectedBand = band;
    this.selectedElement = null;
  }

  protected selectElement(band: BandDefinition, element: ElementDefinition): void {
    this.selectedBand = band;
    this.selectedElement = element;
  }

  protected addBand(): void {
    if (!this.draft) return;
    this.snapshot();
    const band: BandDefinition = {
      id: crypto.randomUUID(),
      type: this.newBandType,
      height: 20,
      visible: true,
      elements: []
    };
    if (this.isGroupBand(band)) band.grouping = {
      expression: this.fieldOptions[0]?.value ?? '',
      sortDirection: 'Ascending',
      summaries: []
    };
    this.draft.bands.push(band);
    this.selectBand(band);
    this.changed(false);
  }

  protected addElement(type: ElementType): void {
    if (!this.selectedBand || !this.draft) return;
    this.snapshot();
    const element: ElementDefinition = {
      id: crypto.randomUUID(),
      type,
      label: type === 'Value' ? this.fieldOptions[0]?.label : undefined,
      valuePath: type === 'Value' ? this.fieldOptions[0]?.value : undefined,
      textTemplate: type === 'Text' ? 'Текст' : undefined,
      sourceUrl: type === 'Image' ? 'https://placehold.co/200x100?text=Image' : undefined,
      layout: {
        x: 5,
        y: 5,
        width: type === 'Line' ? 60 : 45,
        height: type === 'Line' ? 1 : 12,
        zIndex: this.selectedBand.elements.length
      }
    };
    this.selectedBand.elements.push(element);
    this.selectElement(this.selectedBand, element);
    this.changed(false);
  }

  protected deleteElement(): void {
    if (!this.selectedBand || !this.selectedElement) return;
    this.snapshot();
    this.selectedBand.elements = this.selectedBand.elements.filter(x => x.id !== this.selectedElement!.id);
    this.selectedElement = null;
    this.changed(false);
  }

  protected deleteBand(): void {
    if (!this.draft || !this.selectedBand) return;
    this.snapshot();
    this.draft.bands = this.draft.bands.filter(x => x.id !== this.selectedBand!.id);
    this.selectedBand = this.draft.bands[0] ?? null;
    this.selectedElement = null;
    this.changed(false);
  }

  protected setZoom(value: number): void {
    this.zoom = Math.min(1.5, Math.max(.5, value));
  }

  protected elementStyle(element: ElementDefinition): Record<string, string> {
    const style = this.findStyle(element);
    return {
      left: `${element.layout.x * this.pixelScale}px`,
      top: `${element.layout.y * this.pixelScale}px`,
      width: `${element.layout.width * this.pixelScale}px`,
      height: `${element.layout.height * this.pixelScale}px`,
      zIndex: `${element.layout.zIndex}`, ...style?.properties
    };
  }

  protected fieldLabel(path?: string): string | undefined {
    return this.fieldOptions.find(x => x.value === path)?.label;
  }

  protected isGroupBand(band: BandDefinition): boolean {
    return band.type === 'GroupHeader' || band.type === 'GroupFooter';
  }

  protected styleProperty(element: ElementDefinition, property: string, fallback: string): string {
    return this.findStyle(element)?.properties[property] ?? fallback;
  }

  protected styleNumber(element: ElementDefinition, property: string, fallback: number): number {
    return Number.parseFloat(this.styleProperty(element, property, String(fallback))) || fallback;
  }

  protected setStyle(element: ElementDefinition, property: string, value: string): void {
    this.snapshot();
    let style = this.findStyle(element);
    if (!style && this.draft) {
      style = {id: `designer-${element.id}`, cssClass: '', properties: {}};
      this.draft.styles.push(style);
      element.styleId = style.id;
    }
    if (style) style.properties[property] = value;
    this.changed(false);
  }

  protected bringToFront(element: ElementDefinition): void {
    if (!this.selectedBand) return;
    this.snapshot();
    element.layout.zIndex = Math.max(...this.selectedBand.elements.map(x => x.layout.zIndex)) + 1;
    this.changed(false);
  }

  protected startMove(event: PointerEvent, band: BandDefinition, element: ElementDefinition): void {
    event.preventDefault();
    event.stopPropagation();
    this.selectElement(band, element);
    this.snapshot();
    this.activePointer = {
      band,
      element,
      startX: event.clientX,
      startY: event.clientY,
      layout: structuredClone(element.layout),
      resize: false
    };
  }

  protected startResize(event: PointerEvent, band: BandDefinition, element: ElementDefinition): void {
    event.preventDefault();
    event.stopPropagation();
    this.snapshot();
    this.activePointer = {
      band,
      element,
      startX: event.clientX,
      startY: event.clientY,
      layout: structuredClone(element.layout),
      resize: true
    };
  }

  private readonly onPointerMove = (event: PointerEvent): void => {
    if (!this.activePointer) return;
    const move = this.activePointer;
    const dx = (event.clientX - move.startX) / (this.pixelScale * this.zoom);
    const dy = (event.clientY - move.startY) / (this.pixelScale * this.zoom);
    if (move.resize) {
      move.element.layout.width = Math.max(2, move.layout.width + dx);
      move.element.layout.height = Math.max(1, move.layout.height + dy);
    } else {
      move.element.layout.x = Math.max(0, move.layout.x + dx);
      move.element.layout.y = Math.max(0, move.layout.y + dy);
    }
    this.dirty = true;
  };
  private readonly onPointerUp = (): void => {
    if (this.activePointer) {
      this.activePointer = undefined;
      this.changed(false);
    }
  };

  protected changed(takeSnapshot = true): void {
    if (takeSnapshot) this.snapshot();
    this.dirty = true;
  }

  protected undo(): void {
    if (!this.draft || !this.undoStack.length) return;
    this.redoStack.push(JSON.stringify(this.draft));
    this.draft = JSON.parse(this.undoStack.pop()!);
    this.restoreSelection();
    this.dirty = true;
  }

  protected redo(): void {
    if (!this.draft || !this.redoStack.length) return;
    this.undoStack.push(JSON.stringify(this.draft));
    this.draft = JSON.parse(this.redoStack.pop()!);
    this.restoreSelection();
    this.dirty = true;
  }

  protected discard(): void {
    if (!this.source) return;
    this.draft = structuredClone(this.source);
    this.undoStack = [];
    this.redoStack = [];
    this.selectedBand = this.draft.bands[0] ?? null;
    this.selectedElement = null;
    this.dirty = false;
  }

  protected async save(): Promise<void> {
    if (!this.draft) return;
    this.saving = true;
    try {
      const saved = await this.httpClient.request<ReportDefinition>({
        path: `/api/report/update-report/${this.draft.id}`,
        method: 'PUT',
        secure: true,
        type: ContentType.Json,
        format: 'json',
        body: this.draft
      });
      this.source = structuredClone(saved);
      this.draft = structuredClone(saved);
      this.undoStack = [];
      this.redoStack = [];
      this.restoreSelection();
      this.dirty = false;
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.saving = false;
    }
  }

  protected async remove(): Promise<void> {
    if (!this.draft || !confirm(`Удалить шаблон «${this.draft.name}»?`)) return;
    try {
      await this.httpClient.request({
        path: `/api/report/delete-report/${this.draft.id}`,
        method: 'DELETE',
        secure: true
      });
      history.back();
    } catch (error) {
      this.msgService.error(error);
    }
  }

  private snapshot(): void {
    if (!this.draft) return;
    this.undoStack.push(JSON.stringify(this.draft));
    if (this.undoStack.length > 50) this.undoStack.shift();
    this.redoStack = [];
  }

  private findStyle(element: ElementDefinition): StyleDefinition | undefined {
    return this.draft?.styles.find(x => x.id === element.styleId);
  }

  private restoreSelection(): void {
    const bandId = this.selectedBand?.id;
    const elementId = this.selectedElement?.id;
    this.selectedBand = this.draft?.bands.find(x => x.id === bandId) ?? this.draft?.bands[0] ?? null;
    this.selectedElement = this.selectedBand?.elements.find(x => x.id === elementId) ?? null;
  }

  private getReportId(): string | null {
    return this.route.snapshot.queryParamMap.get('reportId');
  }
}
