import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FloatLabelDemoComponent } from './floatlabeldemo.component';
import { FloatlabelDemoRoutingModule } from './floatlabeldemo-routing.module';
import { AutoCompleteModule } from "primeng/autocomplete";
import { CalendarModule } from "primeng/calendar";
import { ChipModule } from "primeng/chip";
import { DropdownModule } from "primeng/dropdown";
import { InputMaskModule } from "primeng/inputmask";
import { InputNumberModule } from "primeng/inputnumber";
import { CascadeSelectModule } from "primeng/cascadeselect";
import { MultiSelectModule } from "primeng/multiselect";
import { TextareaModule } from 'primeng/textarea';
import { InputTextModule } from "primeng/inputtext";
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputGroupModule } from 'primeng/inputgroup';
import { DatePickerModule} from "primeng/datepicker";

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        FloatlabelDemoRoutingModule,
        AutoCompleteModule,
        CalendarModule,
        ChipModule,
        DropdownModule,
        InputMaskModule,
        InputNumberModule,
        CascadeSelectModule,
        MultiSelectModule,
        TextareaModule,
        InputTextModule,
        InputGroupModule,
        InputGroupAddonModule,
        DatePickerModule,
        FloatLabelDemoComponent
    ]
})
export class FloatlabelDemoModule { }
