import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';

function generate2DigitNumber(n) {
    if (n < 10)
        return `0${n}`;
    return `${n}`;
}
function generateTimeStamp(hour, minute) {
    return `${generate2DigitNumber(hour)}:${generate2DigitNumber(minute)}`;
}
function getRandomInt(max, min) {

    const v = Math.floor(Math.random() * Math.floor(max));
    if (v < min)
        return min;
    return v;
}

function getDonationProcessedPerSecChartData() {
    const data = [];
    for (let i = 0; i < 60; i += 2) {
        data.push({ timeStamp: generateTimeStamp(14, i), donationProcessedPerSec: getRandomInt(120, 80) });
    }
    return data;
}

function getDonationReceivedPerSecChartData() {
    const data = [];
    let donatioReceivedPerSec = 0;
    for (let i = 0; i < 60; i += 2) {
        donatioReceivedPerSec += getRandomInt(100, 1);
        data.push({ timeStamp: generateTimeStamp(14, i), donatioReceivedPerSec });
    }
    return data;
}


// Chart demo http://recharts.org/en-US/examples/BiaxialLineChart

export class Home extends Component {

    static displayName = Home.name;

    render() {

        const donationProcessedPerSecChart = (
            <LineChart width={500} height={200} data={getDonationProcessedPerSecChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="donationProcessedPerSec" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="timeStamp" />
                <YAxis />
                <Tooltip />
            </LineChart>
        );

        const donationReceivedPerSecChart = (
            <LineChart width={500} height={200} data={getDonationReceivedPerSecChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
                <Line type="monotone" dataKey="donatioReceivedPerSec" stroke="#8884d8" />
                <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
                <XAxis dataKey="timeStamp" />
                <YAxis />
                <Tooltip />
            </LineChart>
        );


        return (
            <div>                
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.onClear} > Clear </button>

                <div className="card">
                    <div className="card-header">Donation Received Per Second</div>
                    <div className="card-body">
                        {donationReceivedPerSecChart}
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Processed Per Second</div>
                    <div className="card-body">
                        {donationProcessedPerSecChart}
                    </div>
                </div>

                <div className="card">
                    <div className="card-header">Total Donation Received</div>
                    <div className="card-body"> <h4 className="card-title">70 856</h4> </div>
                </div>
                <div className="card">
                    <div className="card-header">Total Donation Processed</div>
                    <div className="card-body"> <h4 className="card-title">50 456</h4> </div>
                    <p className="card-text">Total amount: $ 1 345 678.</p>
                </div>
            </div>
        );
    }
}

/*

                          <p className="card-text">With supporting text below as a natural lead-in to additional content.</p>
 */