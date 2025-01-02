import { Component, OnInit } from '@angular/core';
import { SelectItem, PrimeTemplate } from 'primeng/api';
import { CountryService } from 'projects/oip/src/app/demo/service/country.service';
import { InputText } from 'primeng/inputtext';
import { AutoComplete } from 'primeng/autocomplete';
import { FormsModule } from '@angular/forms';
import { Calendar } from 'primeng/calendar';
import { InputNumber } from 'primeng/inputnumber';
import { Chip } from 'primeng/chip';
import { Slider } from 'primeng/slider';
import { Rating } from 'primeng/rating';
import { ColorPicker } from 'primeng/colorpicker';
import { Knob } from 'primeng/knob';
import { RadioButton } from 'primeng/radiobutton';
import { Checkbox } from 'primeng/checkbox';
import { InputSwitch } from 'primeng/inputswitch';
import { Listbox } from 'primeng/listbox';
import { DropdownModule } from 'primeng/dropdown';
import { MultiSelect } from 'primeng/multiselect';
import { ToggleButton } from 'primeng/togglebutton';
import { SelectButton } from 'primeng/selectbutton';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { ButtonDirective } from 'primeng/button';

@Component({
    templateUrl: './inputdemo.component.html',
    imports: [InputText, AutoComplete, FormsModule, Calendar, InputNumber, Chip, Slider, Rating, ColorPicker, Knob, RadioButton, Checkbox, InputSwitch, Listbox, DropdownModule, MultiSelect, PrimeTemplate, ToggleButton, SelectButton, InputGroup, InputGroupAddon, ButtonDirective]
})
export class InputDemoComponent implements OnInit {

    countries: any[] = [];

    filteredCountries: any[] = [];

    selectedCountryAdvanced: any[] = [];

    valSlider = 50;

    valColor = '#424242';

    valRadio: string = '';

    valCheck: string[] = [];

    valCheck2: boolean = false;

    valSwitch: boolean = false;

    cities: SelectItem[] = [];

    selectedList: SelectItem = { value: '' };

    selectedDrop: SelectItem = { value: '' };

    selectedMulti: any[] = [];

    valToggle = false;

    paymentOptions: any[] = [];

    valSelect1: string = "";

    valSelect2: string = "";

    valueKnob = 20;

    constructor(private countryService: CountryService) { }

    ngOnInit() {
        this.countryService.getCountries().then(countries => {
            this.countries = countries;
        });

        this.cities = [
            { label: 'New York', value: { id: 1, name: 'New York', code: 'NY' } },
            { label: 'Rome', value: { id: 2, name: 'Rome', code: 'RM' } },
            { label: 'London', value: { id: 3, name: 'London', code: 'LDN' } },
            { label: 'Istanbul', value: { id: 4, name: 'Istanbul', code: 'IST' } },
            { label: 'Paris', value: { id: 5, name: 'Paris', code: 'PRS' } }
        ];

        this.paymentOptions = [
            { name: 'Option 1', value: 1 },
            { name: 'Option 2', value: 2 },
            { name: 'Option 3', value: 3 }
        ];
    }

    filterCountry(event: any) {
        const filtered: any[] = [];
        const query = event.query;
        for (let i = 0; i < this.countries.length; i++) {
            const country = this.countries[i];
            if (country.name.toLowerCase().indexOf(query.toLowerCase()) == 0) {
                filtered.push(country);
            }
        }

        this.filteredCountries = filtered;
    }
}
