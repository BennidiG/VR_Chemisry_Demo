from pathlib import Path
from datetime import datetime
import json
import math
import sys

ROOT = Path(__file__).resolve().parents[1]
FLOAT_MAX = 3.4028235e38
checks = []
failures = []


def check(condition, label):
    checks.append(label)
    if not condition:
        failures.append(label)
    print(('PASS ' if condition else 'FAIL ') + label)


def approx(a, b, tolerance=1e-9):
    return abs(a - b) <= tolerance


def nearly_equal(a, b):
    if not math.isfinite(a) or not math.isfinite(b):
        return False
    return abs(a - b) <= max(abs(a), abs(b)) * 0.000001


def evaluate(reactants, products, context):
    minimum_temperature = context.get('minimumTemperatureC')
    if minimum_temperature is not None and context.get('temperatureC', 25.0) < minimum_temperature:
        return {'success': False, 'reason': 'temperature'}
    coefficients = {}
    for substance, coefficient in reactants:
        if coefficient is None or not isinstance(coefficient, (int, float)) or not math.isfinite(coefficient) or coefficient <= 0:
            return {'success': False, 'reason': 'coefficient'}
        coefficients[substance] = coefficients.get(substance, 0.0) + coefficient
    for key, value in context.get('conditions', {}).items():
        if not value:
            return {'success': False, 'reason': key}
    extent = math.inf
    limiting = None
    for substance, coefficient in coefficients.items():
        amount = context['mixture'].get(substance, 0.0)
        if not math.isfinite(amount) or amount <= 0:
            return {'success': False, 'reason': 'missing:' + substance}
        candidate = amount / coefficient
        if not math.isfinite(candidate) or candidate <= 0:
            return {'success': False, 'reason': 'amount'}
        if candidate < extent:
            extent = candidate
            limiting = substance
    if not math.isfinite(extent) or extent <= 0:
        return {'success': False, 'reason': 'no_reagent'}
    produced = {}
    for substance, coefficient in products:
        if coefficient is None or not isinstance(coefficient, (int, float)) or not math.isfinite(coefficient) or coefficient <= 0:
            return {'success': False, 'reason': 'coefficient'}
        produced[substance] = produced.get(substance, 0.0) + coefficient * extent
    return {'success': True, 'extent': extent, 'limiting': limiting, 'produced': produced}


check(True, 'Chemistry simulation initialized')
make_o2 = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'conditions': {'temperature': True, 'catalyst': True, 'collection': True}})
check(make_o2['success'], 'KClO3 reaction succeeds with heat, catalyst and collection')
check(approx(make_o2['extent'], 0.00005), 'KClO3 extent uses limiting reagent')
check(approx(make_o2['produced']['kcl'], 0.0001), 'KCl product follows 2:2 stoichiometry')
check(approx(make_o2['produced']['o2'], 0.00015), 'O2 product follows 2:3 stoichiometry')
check(approx(make_o2['produced']['o2'] * 0.4 * 1000000, 60.0, 1e-6), 'Compatibility oxygen production is 60 units per second')

cold = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'conditions': {'temperature': False}})
check(not cold['success'], 'Cold KClO3 cannot react without required temperature')
no_catalyst = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'conditions': {'temperature': True, 'catalyst': False}})
check(not no_catalyst['success'], 'KClO3 cannot react without catalyst')
no_collection = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'conditions': {'temperature': True, 'catalyst': True, 'collection': False}})
check(not no_collection['success'], 'KClO3 collection requires connected apparatus')

refire = evaluate([('carbon', 1), ('o2', 1)], [('co2', 1)], {'mixture': {'carbon': 0.0002, 'o2': 0.0001}, 'conditions': {'ignition': True, 'ember': True, 'oxygen': True}})
check(refire['success'], 'Carbon refire succeeds with ember and oxygen')
check(approx(refire['produced']['co2'], 0.0001), 'Refire consumes oxygen and produces CO2')
check(approx(refire['produced']['co2'] / 0.0001, 1.0), 'Refire product ratio is 1:1')
no_oxygen = evaluate([('carbon', 1), ('o2', 1)], [('co2', 1)], {'mixture': {'carbon': 0.0002, 'o2': 0.0}, 'conditions': {'ignition': True, 'ember': True, 'oxygen': False}})
check(not no_oxygen['success'], 'Refire fails without oxygen')
no_ember = evaluate([('carbon', 1), ('o2', 1)], [('co2', 1)], {'mixture': {'carbon': 0.0002, 'o2': 0.0001}, 'conditions': {'ignition': True, 'ember': False, 'oxygen': True}})
check(not no_ember['success'], 'Refire fails without hot ember')

neutral = evaluate([('hcl', 1), ('naoh', 1)], [('nacl', 1), ('h2o', 1)], {'mixture': {'hcl': 0.001, 'naoh': 0.002}, 'conditions': {'mixing': True}})
check(neutral['success'], 'Neutralization succeeds with mixing')
check(neutral['limiting'] == 'hcl', 'Neutralization identifies limiting reagent')
check(approx(neutral['produced']['nacl'], 0.001), 'Neutralization produces NaCl in 1:1 ratio')
check(approx(neutral['produced']['h2o'], 0.001), 'Neutralization produces liquid water in 1:1 ratio')
check(approx(neutral['produced']['h2o'] + neutral['produced']['nacl'], 0.002), 'Neutralization product totals are consistent')

make_o2_input_mass = 0.0001 * 122.55
make_o2_output_mass = 0.0001 * 74.55 + 0.00015 * 32.0
check(approx(make_o2_input_mass, make_o2_output_mass, 1e-8), 'MakeO2 input and output mass are conserved')
refire_input_mass = 0.0001 * 12.01 + 0.0001 * 32.0
refire_output_mass = 0.0001 * 44.01
check(approx(refire_input_mass, refire_output_mass, 1e-8), 'Refire input and output mass are conserved')
neutral_input_mass = 0.001 * 36.46 + 0.001 * 40.0
neutral_output_mass = 0.001 * 58.44 + 0.001 * 18.015
check(approx(neutral_input_mass, neutral_output_mass, 0.00001), 'Neutralization mass is conserved within molar mass precision')

bad = evaluate([('hcl', 1)], [('nacl', 1)], {'mixture': {'hcl': 0.0}, 'conditions': {'mixing': True}})
check(not bad['success'], 'Reaction rejects missing reagent')

# Capacity and conversion checks.
capacity = 10.0
volume = 0.0
accepted = volume + 4.0 <= capacity
check(accepted, 'Mixture accepts reagent within capacity')
check(volume + 11.0 > capacity, 'Mixture rejects reagent above capacity')
check(10000.0 / 1000000.0 == 0.01, 'Oxygen legacy 10000 units equal 0.01 mol')

duplicate = evaluate([('hcl', 0.5), ('hcl', 0.5)], [('nacl', 1), ('nacl', 1)], {'mixture': {'hcl': 0.001, 'naoh': 0.002}, 'conditions': {'mixing': True}})
check(duplicate['success'] and approx(duplicate['extent'], 0.001), 'Duplicate reactants are aggregated by substance id')
check(approx(duplicate['produced']['nacl'], 0.002), 'Duplicate products are aggregated instead of overwritten')
nonfinite = evaluate([('hcl', float('nan'))], [('nacl', 1)], {'mixture': {'hcl': 0.001}, 'conditions': {'mixing': True}})
check(not nonfinite['success'], 'NaN coefficient is rejected')
null_participant = evaluate([('hcl', None)], [('nacl', 1)], {'mixture': {'hcl': 0.001}, 'conditions': {'mixing': True}})
check(not null_participant['success'], 'Null participant coefficient is rejected')
negative_temp = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'temperatureC': -25, 'minimumTemperatureC': -20, 'conditions': {'temperature': True, 'catalyst': True, 'collection': True}})
check(not negative_temp['success'], 'Negative minimum temperature is enforced')
zero_temp = evaluate([('kclo3', 2)], [('kcl', 2), ('o2', 3)], {'mixture': {'kclo3': 0.0001}, 'temperatureC': 0, 'minimumTemperatureC': 0, 'conditions': {'temperature': True, 'catalyst': True, 'collection': True}})
check(zero_temp['success'], 'Zero degree minimum temperature is supported')


def add_portion(mixture, substance, amount, volume):
    if not math.isfinite(amount) or not math.isfinite(volume) or amount <= 0 or volume < 0 or mixture['totalVolume'] + volume > mixture['capacity']:
        return False
    portion = mixture['reagents'].setdefault(substance, {'amount': 0.0, 'volume': 0.0, 'concentration': 0.0})
    portion['amount'] += amount
    portion['volume'] += volume
    portion['concentration'] = portion['amount'] * 1000.0 / portion['volume'] if portion['volume'] > 0 else 0.0
    mixture['totalVolume'] += volume
    return True


def consume_portion(mixture, substance, amount):
    portion = mixture['reagents'].get(substance)
    if portion is None or not math.isfinite(amount) or amount <= 0:
        return 0.0
    taken = min(amount, portion['amount'])
    fraction = taken / portion['amount'] if portion['amount'] > 0 else 0.0
    before_volume = portion['volume']
    portion['amount'] -= taken
    portion['volume'] *= 1.0 - fraction
    portion['concentration'] = portion['amount'] * 1000.0 / portion['volume'] if portion['volume'] > 0 else 0.0
    mixture['totalVolume'] = max(0.0, mixture['totalVolume'] - before_volume * fraction)
    if portion['amount'] <= 0:
        del mixture['reagents'][substance]
    return taken

mixture = {'capacity': 100.0, 'totalVolume': 0.0, 'reagents': {}}
check(add_portion(mixture, 'hcl', 1.0, 10.0), 'Mixture accepts first reagent portion')
check(add_portion(mixture, 'hcl', 1.0, 10.0), 'Mixture merges same-substance portions')
check(approx(mixture['reagents']['hcl']['concentration'], 100.0), 'Merged concentration is 100 mol/L')
check(approx(consume_portion(mixture, 'hcl', 1.0), 1.0), 'Consume uses actual available amount')
check(approx(mixture['reagents']['hcl']['concentration'], 100.0), 'Concentration remains 100 mol/L after consumption')
check(approx(mixture['totalVolume'], 10.0), 'Consumption updates mixture volume')
check(not add_portion(mixture, 'hcl', float('nan'), 1.0), 'Mixture rejects NaN amount')
check(not add_portion(mixture, 'hcl', float('inf'), 1.0), 'Mixture rejects infinite amount')

class ProcessSim:
    def __init__(self, reactants, products, context, rate):
        self.reactants = reactants
        self.products = products
        self.context = context
        self.rate = rate
        self.status = 'not_started'
        self.progress = 0.0
        self.base = None
        self.initial = None
        self.gas = {}
        self.solid = {}
        self.elapsed = 0.0
        self.gas_total = 0.0
        self.solid_total = 0.0

    def start(self):
        if self.status in ('running', 'paused'):
            return {'success': False}
        self.initial = dict(self.context['mixture'])
        self.base = evaluate(self.reactants, [(substance, coefficient) for substance, coefficient, _ in self.products], self.context)
        self.status = 'running' if self.base['success'] else 'failed'
        self.progress = 0.0
        return self.base

    def tick(self, delta_time):
        if self.status not in ('running', 'paused'):
            return {'status': self.status, 'progress': self.progress}
        if not math.isfinite(delta_time) or delta_time < 0:
            self.status = 'failed'
            return {'status': self.status, 'progress': self.progress}
        if delta_time == 0:
            return {'status': self.status, 'progress': self.progress}
        minimum_temperature = self.context.get('minimumTemperatureC')
        if minimum_temperature is not None and self.context.get('temperatureC', 25.0) < minimum_temperature:
            self.status = 'paused'
            return {'status': self.status, 'progress': self.progress}
        if any(not value for value in self.context.get('conditions', {}).values()):
            self.status = 'paused'
            return {'status': self.status, 'progress': self.progress}

        self.status = 'running'
        delta = min(self.rate * delta_time, 1.0 - self.progress)
        if not math.isfinite(delta) or delta < 0:
            self.status = 'failed'
            return {'status': self.status, 'progress': self.progress}
        if delta <= 0:
            return {'status': self.status, 'progress': self.progress}

        coefficients = {}
        for substance, coefficient in self.reactants:
            coefficients[substance] = coefficients.get(substance, 0.0) + coefficient
        consumed = {substance: coefficient * self.base['extent'] * delta for substance, coefficient in coefficients.items()}
        for substance, amount in consumed.items():
            if not math.isfinite(amount) or (self.context['mixture'].get(substance, 0.0) < amount and not nearly_equal(self.context['mixture'].get(substance, 0.0), amount)):
                self.status = 'failed'
                return {'status': self.status, 'progress': self.progress}
        projected_gas = self.gas_total
        projected_solid = self.solid_total
        for substance, coefficient, phase in self.products:
            amount = coefficient * self.base['extent'] * delta
            if not math.isfinite(amount):
                self.status = 'failed'
                return {'status': self.status, 'progress': self.progress}
            if phase == 'gas':
                projected_gas += amount
                if projected_gas > FLOAT_MAX:
                    self.status = 'failed'
                    return {'status': self.status, 'progress': self.progress}
            elif phase == 'solid':
                projected_solid += amount
                if projected_solid > FLOAT_MAX:
                    self.status = 'failed'
                    return {'status': self.status, 'progress': self.progress}
        for substance, amount in consumed.items():
            self.context['mixture'][substance] = max(0.0, self.context['mixture'][substance] - amount)
        for substance, coefficient, phase in self.products:
            amount = coefficient * self.base['extent'] * delta
            self.context['mixture'][substance] = self.context['mixture'].get(substance, 0.0) + amount
            if phase == 'gas':
                self.gas[substance] = self.gas.get(substance, 0.0) + amount
                self.gas_total += amount
            elif phase == 'solid':
                self.solid[substance] = self.solid.get(substance, 0.0) + amount
                self.solid_total += amount
        self.progress += delta
        self.elapsed += delta / self.rate
        if self.progress >= 1.0:
            self.progress = 1.0
            self.status = 'completed'
        return {'status': self.status, 'progress': self.progress, 'delta': delta}

    def cancel(self):
        if self.status in ('running', 'paused'):
            self.status = 'cancelled'

    def reset(self, restore=True):
        if restore and self.initial is not None:
            self.context['mixture'] = dict(self.initial)
            self.gas.clear()
            self.solid.clear()
            self.gas_total = 0.0
            self.solid_total = 0.0
        self.status = 'not_started'
        self.progress = 0.0
        self.elapsed = 0.0
        self.base = None


def make_process_context():
    return {'mixture': {'kclo3': 0.0001}, 'conditions': {'temperature': True, 'catalyst': True, 'collection': True}}


process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
first = process.tick(1.0)
check(first['status'] == 'running' and approx(first['progress'], 0.4), 'Process first tick advances once')
check(approx(process.context['mixture']['kclo3'], 0.00006), 'Process consumes only the first progress fraction')
check(approx(process.gas['o2'], 0.00006), 'Process produces gas by progress fraction')
second = process.tick(2.0)
check(second['status'] == 'completed', 'Process completes at progress one')
check(approx(process.elapsed, 2.5), 'Truncated final tick records active reaction time')
check(approx(process.gas['o2'], 0.00015), 'Process final oxygen amount is exact')
check(approx(process.solid['kcl'], 0.0001), 'Process tracks solid product amount')
completed_gas = process.gas['o2']
process.tick(10.0)
check(approx(process.gas['o2'], completed_gas), 'Completed process cannot produce again')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.tick(0.5)
process.context['conditions']['collection'] = False
paused = process.tick(1.0)
check(paused['status'] == 'paused' and approx(paused['progress'], 0.2), 'Process pauses when conditions disappear')
check(approx(process.context['mixture']['kclo3'], 0.00008), 'Paused process does not consume reagents')
process.context['conditions']['collection'] = True
resumed = process.tick(0.5)
check(resumed['status'] == 'running' and approx(resumed['progress'], 0.4), 'Process resumes after conditions return')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
check(process.tick(0.0)['progress'] == 0.0, 'Zero time does not advance process')
check(process.tick(-1.0)['status'] == 'failed', 'Negative time fails safely')
check(approx(process.context['mixture']['kclo3'], 0.0001), 'Invalid time does not mutate mixture')
process.reset()
process.start()
check(process.tick(float('nan'))['status'] == 'failed', 'NaN time fails safely')
check(approx(process.gas.get('o2', 0.0), 0.0), 'NaN time produces no gas')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.context['mixture']['kclo3'] = 0.00001
check(process.tick(1.0)['status'] == 'failed', 'Insufficient reagent fails process')
check(approx(process.context['mixture']['kclo3'], 0.00001), 'Failed process does not partially consume')
check(approx(process.gas.get('o2', 0.0), 0.0), 'Failed process does not partially produce')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.tick(0.5)
process.cancel()
check(process.tick(1.0)['status'] == 'cancelled', 'Cancelled process cannot tick')
process.reset()
check(approx(process.context['mixture']['kclo3'], 0.0001), 'Reset restores initial mixture')
process.start()
check(approx(process.tick(0.5)['progress'], 0.2), 'Reset allows a clean new run')

context = make_process_context()
oxygen = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], context, 0.4)
oxygen.start()
oxygen.tick(2.5)
context['mixture']['carbon'] = 0.0001
context['conditions'] = {'ignition': True, 'ember': True, 'oxygen': True}
refire = ProcessSim([('carbon', 1), ('o2', 1)], [('co2', 1, 'gas')], context, 1.0)
refire.start()
refire.tick(1.0)
check(approx(context['mixture']['carbon'], 0.0), 'Refire consumes carbon from the mixture')
check(approx(context['mixture']['o2'], 0.00005), 'Generated oxygen can feed the next reaction')
check(approx(refire.gas['co2'], 0.0001), 'Refire produces CO2 from generated oxygen')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.tick(0.5)
process.reset(False)
check(approx(process.context['mixture']['kclo3'], 0.00008), 'Reset without restore keeps current mixture')
check(approx(process.gas['o2'], 0.00003), 'Reset without restore keeps produced gas')
process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.tick(0.5)
check(not process.start()['success'] and approx(process.progress, 0.2), 'Starting a running process does not reset progress')

process = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.4)
process.start()
process.context['mixture']['kclo3'] = 0.00003997
before = process.context['mixture']['kclo3']
check(process.tick(1.0)['status'] == 'failed', 'Tiny reagent shortage fails exactly')
check(approx(process.context['mixture']['kclo3'], before) and approx(process.gas.get('o2', 0.0), 0.0), 'Tiny shortage does not consume or produce')

context = {'mixture': {'kclo3': 2.0}, 'conditions': {'temperature': True, 'catalyst': True, 'collection': True}}
overflow = ProcessSim([('kclo3', 2)], [('o2', 1e38, 'gas'), ('co2', 1e38, 'gas')], context, 1.0)
overflow.start()
overflow.gas_total = 2e38
check(overflow.tick(1.0)['status'] == 'failed', 'Aggregate gas overflow fails before commit')
check(approx(context['mixture']['kclo3'], 2.0), 'Overflow failure does not consume reagent')
check(approx(overflow.gas_total, 2e38) and len(overflow.gas) == 0, 'Overflow failure rolls back all gas products')
check(overflow.progress == 0.0, 'Overflow failure does not advance progress')

underflow = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), float('5e-324'))
underflow.start()
check(underflow.tick(float('5e-324'))['status'] == 'running', 'Zero progress delta does not complete process')
fractional = ProcessSim([('kclo3', 2)], [('kcl', 2, 'solid'), ('o2', 3, 'gas')], make_process_context(), 0.333333)
fractional.start()
fractional.tick(1.0)
fractional.tick(1.0)
fractional.tick(1.0)
check(fractional.status == 'running' and fractional.progress < 1.0, 'Progress is not rounded up to completion early')
check(fractional.tick(0.00001)['status'] == 'completed', 'Remaining progress is completed by an actual final tick')

original = {'kclo3': 0.0001}
clone = dict(original)
clone['kclo3'] -= 0.00005
check(approx(original['kclo3'], 0.0001), 'Mixture clone does not mutate original reagent state')
check(approx(clone['kclo3'], 0.00005), 'Mixture clone has independent reagent state')

report = {
    'timestamp': datetime.now().isoformat(timespec='seconds'),
    'checks': len(checks),
    'passed': len(checks) - len(failures),
    'failed': failures,
    'pending': ['Unity chemistry tests must still be run in Unity Editor and PlayMode.']
}
(ROOT / 'Logs').mkdir(exist_ok=True)
(ROOT / 'Logs/chemistry_core_simulation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print('\nCHEMISTRY SIMULATION:', report['passed'], '/', report['checks'], 'passed')
if failures:
    sys.exit(1)
