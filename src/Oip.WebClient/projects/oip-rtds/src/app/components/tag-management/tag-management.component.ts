import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { TagModule } from 'primeng/tag';
import { ConfirmationService, SelectItem, SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { DatePipe, NgIf } from '@angular/common';
import { TagManagementModule } from "../../api/TagManagementModule";
import { DropdownModule } from "primeng/dropdown";
import { Checkbox } from "primeng/checkbox";
import { Calendar } from "primeng/calendar";
import { Button } from "primeng/button";
import { InputText } from "primeng/inputtext";
import { InputNumber } from "primeng/inputnumber";
import { CreateTagDto, TagEntity, TagTypes } from "../../api/data-contracts";
import { FormsModule } from "@angular/forms";
import { Tooltip } from "primeng/tooltip";
import { Textarea } from "primeng/textarea";
import { Toolbar } from "primeng/toolbar";
import { IconField } from "primeng/iconfield";
import { InputIcon } from "primeng/inputicon";
import { Dialog } from "primeng/dialog";
import { Select } from "primeng/select";
import { ConfirmDialogModule } from 'primeng/confirmdialog';

export interface WeatherSettingsDto {
}

@Component({
  selector: 'tag-management',
  templateUrl: './tag-management.component.html',
  providers: [TagManagementModule, ConfirmationService],
  imports: [
    NgIf,
    TableModule,
    SharedModule,
    TagModule,
    SecurityComponent,
    DropdownModule,
    Checkbox,
    Button,
    InputText,
    InputNumber,
    DatePipe,
    FormsModule,
    Tooltip,
    Textarea,
    Toolbar,
    IconField,
    InputIcon,
    Dialog,
    Select, ConfirmDialogModule
  ],
})
export class TagManagement extends BaseComponent<WeatherSettingsDto> implements OnInit, OnDestroy, Feature {
  controller: string = 'tag-management';
  tagManagementModuleDataService = inject(TagManagementModule);
  protected readonly confirmationService: ConfirmationService = inject(ConfirmationService);
  tags: TagEntity[] = [];
  selectedTag: TagEntity;
  createTag: TagEntity = {} as TagEntity;
  valueTypeOptions: SelectItem[] = Object.keys(TagTypes).map(key => ({
    label: TagTypes[key as keyof typeof TagTypes],
    value: TagTypes[key as keyof typeof TagTypes]
  }));
  createTagDialogVisible: boolean = false;
  createDialogSelectedType: any;

  getTagTypeName(type: TagTypes): string {
    return TagTypes[type];
  }

  async ngOnInit() {
    await super.ngOnInit();
    await this.refresh();
  }

  async refresh() {
    this.tags = await this.tagManagementModuleDataService.tagManagementGetTagsByFilterList({ filter: '%' });
  }

  async saveTag() {
    try {
      let createTag = {
        tagId: this.selectedTag.id,
        name: this.selectedTag.name,
        valueType: this.selectedTag.valueType,
        interface: this.selectedTag.interface,
        descriptor: this.selectedTag.descriptor,
        uom: this.selectedTag.uom,
        instrumentTag: this.selectedTag.instrumentTag,
        enabled: this.selectedTag.enabled,
        compressing: this.selectedTag.compressing,
        compressionMinTime: this.selectedTag.compressionMinTime,
        compressionMaxTime: this.selectedTag.compressionMaxTime,
        digitalSet: this.selectedTag.digitalSet,
        step: this.selectedTag.step,

        timeCalculation: this.selectedTag.timeCalculation,
        errorCalculation: this.selectedTag.errorCalculation,
        /** User-defined calculation or formula associated with the tag's value. */
        valueCalculation: this.selectedTag.valueCalculation,
      } as CreateTagDto;

      await this.tagManagementModuleDataService.tagManagementEditTagCreate(createTag);
      this.msgService.success("Tag saved successfully.");
    } catch
      (error) {
      this.msgService.error(error);
    }
  }


  showCreateTagDialog() {
    this.createTagDialogVisible = true;
  }

  cancelCreateDialogClick() {
    this.createTagDialogVisible = false;
  }

  async saveCreateTagDialogClick() {
    try {
      await this.tagManagementModuleDataService.tagManagementAddTagCreate(this.createTag as CreateTagDto);
      this.createTagDialogVisible = false;
      this.msgService.success("Tag added successfully.");

    } catch (error) {
      this.msgService.error(error);
    }
  }

  deleteTag(tag: TagEntity) {
    this.confirmationService.confirm({
      header: 'Warning',
      icon: 'pi pi-trash',
      message: 'Are you sure you want to delete this record?',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: 'Delete',
        severity: 'danger'
      },
      accept: async () => {
        this.msgService.success('Tag deleted successfully.');
        await this.refresh();
      }
    });
  }
}
