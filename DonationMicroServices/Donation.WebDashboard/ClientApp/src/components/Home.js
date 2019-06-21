import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';
import ReactTable from "react-table";
import "react-table/react-table.css";

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

    timerId = null;
    refreshTimeOut = 1000 * 4;

    state = {        
        systemActivitySummary: {
            pushedDonationActivitySummaryDictionary: { },
            lastMessage: "No message yet",
        }
    };

    componentDidMount() {
        this.timerId = setInterval(() => {
            console.log(`About to refresh...`);

            fetch('api/SystemActivities/GetSystemActivitySummary')
                .then(response => response.json())
                .then(data => {
                    
                    this.updateState('systemActivitySummary', data);                    
                });
            
        }, this.refreshTimeOut)
    }

    updateState = (property, value) => {
        this.setState({ ...this.state, [property]: value }, () => {
            //console.log(`state: ${JSON.stringify(this.state)}`);
        });
    }

    getPushedDonationActivitySummaryTable = () => {
        let sTotal = 0;
        let sItemPerSecond = 0;
        let sCaption = null;
        const machineNames = Object.keys(this.state.systemActivitySummary.pushedDonationActivitySummaryDictionary);
        const r = machineNames.map(
            (machineName) => {
                const machineInfo = this.state.systemActivitySummary.pushedDonationActivitySummaryDictionary[machineName];
                sTotal += machineInfo.total;
                sItemPerSecond += machineInfo.itemPerSecond;
                if (sCaption === null)
                    sCaption = machineInfo.caption;
                return {
                    machineName: machineInfo.machineName,
                    caption: machineInfo.caption,
                    total: machineInfo.total,
                    itemPerSecond: machineInfo.itemPerSecond,
                };
            });

        if (machineNames.length > 0) {
            sItemPerSecond = sItemPerSecond / machineNames.length; // compute average
        }
            
        r.push({ // Add summation row
            machineName: 'Total',
            caption: sCaption,
            total: sTotal,
            itemPerSecond: sItemPerSecond,
        });
        return r;
    }

    // https://www.npmjs.com/package/react-table
    // https://codesandbox.io/s/react-table-simple-table-hpduw
    // With colors
    // https://codesandbox.io/s/github/tannerlinsley/react-table/tree/master/archives/v6-examples/react-table-cell-renderers
    renderPushedDonationActivitySummaryTable = () => {
        const data = this.getPushedDonationActivitySummaryTable();       
        if (data.length === 0)
            return null

        return (
                <ReactTable
                    data={data}
                    columns={[
                        {
                            Header: "Donation Pushed To Queue",
                            columns: [
                                {
                                    Header: "Machine Name",
                                    id: "machineName",
                                    accessor: d => d.machineName.replace("person", "")
                                },
                                { Header: "Activity", accessor: "caption" },
                                { Header: "Total", accessor: "total" },
                                { Header: "Donation/S", accessor: "itemPerSecond" }
                            ]
                        }
                    ]}
                    defaultPageSize={3}
                    className="-striped -highlight"
                    showPagination={false}
            />
        );
    }

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
                    <div className="card-header">Message</div>
                    <div className="card-body"> <h4 className="card-title">
                        {this.state.systemActivitySummary.lastMessage}
                    </h4> </div>
                </div>

                <div className="card">
                    <div className="card-header">Donation Pushed</div>
                    <div className="card-body"> <h4 className="card-title">
                        {this.renderPushedDonationActivitySummaryTable()}
                    </h4> </div>
                </div>
                <div className="card">
                    <div className="card-header">Total Donation Processed</div>
                    <div className="card-body"> <h4 className="card-title">
                        {123456}
                    </h4> </div>
                    <p className="card-text">Total amount: $ 1 345 678.</p>
                </div>
            </div>
        );
    }
}

/*

                          <p className="card-text">With supporting text below as a natural lead-in to additional content.</p>
 */
