import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip } from 'recharts';
import ReactTable from "react-table";
import "react-table/react-table.css";
import {
    getDonationProcessedPerSecChartData,
    getDonationReceivedPerSecChartData
} from './generateDashboardData'

/*
recharts
     http://recharts.org/en-US/examples/BiaxialLineChart
React-Table    
     https://www.npmjs.com/package/react-table
     https://codesandbox.io/s/react-table-simple-table-hpduw
     https://codesandbox.io/s/github/tannerlinsley/react-table/tree/master/archives/v6-examples/react-table-cell-renderers
 */

export class Home extends Component {

    static displayName = Home.name;

    timerId = null;
    refreshTimeOut = 1000 * 5;    
    summaryTableDefaultPageSize = 6;

    state = {
        systemActivitySummary: {
            donationCountryBreakdown                       : {},
            donationSentToEndPointActivitySummaryDictionary: {}, 
            donationEnqueuedActivitySummaryDictionary      : {},
            dashboardResourceActivitySummaryDictionary     : {},            
            donationProcessedActivitySummaryDictionary     : {},
            donationInfoSummaryDictionary                  : {},
            donationErrorsSummaryDictionary                : {},
            lastMessage                                    : "No message yet",
            
        },
        autoRefreshOn: false,
        donationCountryBreakdownMinimunAmountForDisplay: 1000,
    };

    // Return a promise
    clearAllErrors = () => {

        return fetch('api/SystemActivities/GetSystemActivityClearError').then(response => response.json())
            .then(data => {
                console.log(`clearAllErrors data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    // Return a promise
    clearAll = () => {

        return fetch('api/SystemActivities/GetSystemActivityClearAll').then(response => response.json())
            .then(data => {
                console.log(`clearAll data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    // Return a promise
    reloadData = () => {

        return fetch('api/SystemActivities/GetSystemActivitySummary').then(response => response.json())
            .then(data => {
                console.log(`reloadData data:${JSON.stringify(data)}`);
                this.updateState('systemActivitySummary', data);
            });
    }

    componentDidMount() {
        
        this.reloadData().then(() => {

            this.setAutoRefresh(true);
        });        
    }

    updateState = (property, value) => {

        this.setState({ ...this.state, [property]: value });
    }

    reverseAutoRefresh = () => {

        this.setAutoRefresh(!this.state.autoRefreshOn);
    }

    getAutoRefreshStatus = () => {

        return this.state.autoRefreshOn ? "On" : "Off";
    }

    setAutoRefresh = (on) => {

        if (on) {
            this.timerId = setInterval(this.reloadData, this.refreshTimeOut);
        }
        else {
            clearTimeout(this.timerId);
        }
        this.updateState('autoRefreshOn', on);
    }    

    getActivitySummaryTable = (activitySummaryDictionary) => {

        let sTotal         = 0;
        let sItemPerSecond = 0;
        let sCaption       = null;
        const machineNames = Object.keys(activitySummaryDictionary);

        const r = machineNames.map((machineName) => {

            const machineInfo  = activitySummaryDictionary[machineName];
            sTotal            += machineInfo.total;
            sItemPerSecond    += machineInfo.itemPerSecond;

            if (sCaption === null) sCaption = machineInfo.caption;

            return {
                machineName  : machineInfo.machineName,
                caption      : machineInfo.caption,
                total        : machineInfo.total,
                itemPerSecond: machineInfo.itemPerSecond,
                jsonData     : machineInfo.jsonData,
                messages     : machineInfo.messages, // This is an array of string
            };
        });

        r.push({ // Add summation row
            machineName  : 'Total',
            caption      : sCaption,
            total        : sTotal,
            itemPerSecond: sItemPerSecond,
        });
        return r;
    }

    getDonationInfoActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationInfoSummaryDictionary);
    }

    getDonationErrorsActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationErrorsSummaryDictionary);
    }

    getDonationProcessedpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary);
    }

    getDonationSentToEndpointActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary);
    }

    getDonationEnqueuedActivitySummaryTable = () => {

        return this.getActivitySummaryTable(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary);
    }
    
    //getDashboardResourceActivitySummaryTable = () => {

    //    return this.getActivitySummaryTable(this.state.systemActivitySummary.dashboardResourceActivitySummaryDictionary);
    //}

    renderDonationInfoActivitySummaryTable = () => {

        const data = this.getDonationInfoActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Info",
                    columns: this.getColumnsForMessagesTable()
                }]}
                defaultPageSize={this.summaryTableDefaultPageSize+2}
                className="-striped -highlight"
                showPagination={false}
                SubComponent={row => {
                    
                    var messages = row.original.messages;
                    var messagesHtml = messages.map((message, index) => {
                        return <li key={index}>{message}</li>;
                    });                    
                    return (
                        <ul style={{ padding: "20px" }}>
                            {messagesHtml}
                        </ul>
                    );
                }}
            />
        );
    }

    renderDonationErrorsActivitySummaryTable = () => {

        const data = this.getDonationErrorsActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Errors",
                        columns: this.getColumnsForMessagesTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
                SubComponent={row => {
                    var messages = row.original.message;
                    var messagesHtml = messages.map((message, index) => {
                        return <li key={index}>{message}</li>;
                    });
                    return (
                        <ul style={{ padding: "20px" }}>
                            {messagesHtml}
                        </ul>
                    );
                }}
            />
        );
    }

    getColumnsForMessagesTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Messages", id: "message", accessor: d => d.message ? d.message[d.message.length - 1] : '' } // Return the last message that arrived
    ]

    getColumnsForDonationPerSecondTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName,
        },
        { Header: "Total", accessor: "total" },
        { Header: "Donation/S", accessor: "itemPerSecond" }
    ];

    renderDonationProcessedActivitySummaryTable = () => {

        const data = this.getDonationProcessedpointActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Processed",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationSentToEndpointActivitySummaryTable = () => {

        const data = this.getDonationSentToEndpointActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                        Header: "Donation Sent To Endpoint",
                        columns: this.getColumnsForDonationPerSecondTable()
                }]}                
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }

    renderDonationEnqueuedActivitySummaryTable = () => {

        const data = this.getDonationEnqueuedActivitySummaryTable();
        if (data.length === 0)
            return null

        return (
            <ReactTable
                data={data}
                columns={[{
                    Header: "Donation Enqueued",
                    columns: this.getColumnsForDonationPerSecondTable()
                }]}
                defaultPageSize={this.summaryTableDefaultPageSize}
                className="-striped -highlight"
                showPagination={false}
            />
        );
    }
    
    getColumnsForJsonDataTable = () => [
        {
            Header: "Machine Name",
            id: "machineName",
            accessor: d => d.machineName.replace("person", ""),
        },
        { Header: "Total", accessor: "total" },
        { Header: "Json Data", accessor: "jsonData" }
    ];
    
    //renderDashboardResourceActivitySummaryTable = () => {

    //    const data = this.getDashboardResourceActivitySummaryTable();
    //    if (data.length === 0)
    //        return null

    //    return (
    //        <ReactTable
    //            data={data}
    //            columns={[{
    //                Header: "Dashboard Country Aggregate",
    //                columns: this.getColumnsForJsonDataTable()
    //            }]}
    //            defaultPageSize={this.summaryTableDefaultPageSize}
    //            className="-striped -highlight"
    //            showPagination={false}
    //        />
    //    );
    //}

    getDonationMachineCount = (dictionary) => {

        const machineNames = Object.keys(dictionary);
        return machineNames.length;
    };

    getDonationMachineName = (dictionary, machineIndex) => {

        const machineNames = Object.keys(dictionary);
        if (machineNames.length) {
            const machineName = machineNames[machineIndex];
            return machineName;
        }
        return null;
    };
    
    getCountryBreakDownChartData = () => {

        const donationCountryBreakdown = this.state.systemActivitySummary.donationCountryBreakdown;
        const countries = Object.keys(donationCountryBreakdown);
        const data = [];
        countries.forEach((country) => {
            const amount = Math.round(donationCountryBreakdown[country]);
            if (amount > this.state.donationCountryBreakdownMinimunAmountForDisplay) {
                data.push({ country, amount });
            }            
        });
        data.sort((a, b) => (a.amount < b.amount) ? 1 : -1);
        return data;
    }

    getDonationChartData = (dictionary, machineIndex) => {
        
        const machineNames = Object.keys(dictionary);
        if (machineNames.length) {
            const machineName = machineNames[machineIndex];
            if (machineName) {
                const history = dictionary[machineName].history;
                const data = history.map((e) => {
                    return { timeStamp: e.chartLabel, value: e.total };
                });
                return data;
            }
            else {
                // Machine #2 may not be ready
            }
        }
        return [];
    };

    getDonationSentToEndPointChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary, machineIndex);
    };

    getDonationEnqueuedChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary, machineIndex);
    };

    getDonationProcessedChartData = (machineIndex) => {

        return this.getDonationChartData(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary, machineIndex);
    };

    getDonationProcessedMachineName = (machineIndex) => {
        const chartWidth = 501;
        const chartHeight = 175;
        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationProcessedChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationEnqueuedMachineName = (machineIndex) => {
        const chartWidth = 501;
        const chartHeight = 175;
        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationEnqueuedChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationSentToEndPointMachineName = (machineIndex) => {
        const chartWidth = 501;
        const chartHeight = 175;
        return <LineChart width={chartWidth} height={chartHeight} data={this.getDonationSentToEndPointChartData(machineIndex)} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="value" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="timeStamp" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };

    getDonationCountryBreakdownChart = () => {
        const chartWidth = 1000;
        const chartHeight = 175;
        return <LineChart width={chartWidth} height={chartHeight} data={this.getCountryBreakDownChartData()} margin={{ top: 5, right: 20, bottom: 5, left: 0 }}>
            <Line type="monotone" dataKey="amount" stroke="#8884d8" />
            <CartesianGrid stroke="#ccc" strokeDasharray="5 5" />
            <XAxis dataKey="country" />
            <YAxis />
            <Tooltip />
        </LineChart>;
    };
    
    getDonationPerformanceInfoChartsTR = (dictionary, title, generateChartCallBack) => {
        let max = this.getDonationMachineCount(dictionary);
        let html = [];
        for (let i = 0; i < max; i++) {
            html.push(<td key={`${title}-${i}`}>
                <div className="card">
                    <div className="card-header">{title}, machine {i} {this.getDonationMachineName(dictionary, i)}</div>
                    <div className="card-body">
                        {generateChartCallBack(i)}                        
                    </div>
                </div>
            </td>);
        }
        return html;
    }

    getDonationPerformanceInfoCharts = () => {
        return <table>
            <tbody>
            <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationSentToEndPointActivitySummaryDictionary, 'Donation Sent', this.getDonationSentToEndPointMachineName)}
            </tr>
            <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationEnqueuedActivitySummaryDictionary, 'Donation Enqueued', this.getDonationEnqueuedMachineName)}
            </tr>
            <tr>
                    {this.getDonationPerformanceInfoChartsTR(this.state.systemActivitySummary.donationProcessedActivitySummaryDictionary, 'Donation Processed ', this.getDonationProcessedMachineName)}
            </tr>
            </tbody>
        </table >
    }

    onDonationCountryBreakdownMinimunAmountForDisplayChange = (e) => {

        this.updateState('donationCountryBreakdownMinimunAmountForDisplay', e.target.value);
    }

    render() {
        // console.log(`getCountryBreakDownChartData: ${JSON.stringify(this.getCountryBreakDownChartData())}`);

        return (
            <div>
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.reverseAutoRefresh} > AutoRefresh: {this.getAutoRefreshStatus()} </button>
                &nbsp;
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.reloadData} > Refresh </button>
                &nbsp;
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.clearAllErrors} > Clear Errors </button>
                &nbsp;
                <button type="button" className="btn btn-primary  btn-sm " onClick={this.clearAll} > Clear All </button>
                &nbsp;
                {new Date().toString()}

                {this.getDonationPerformanceInfoCharts()}

                <div className="card">
                    <div className="card-header">Countries Break Down &nbsp;&nbsp;&nbsp;
                        <small>
                            ( Minimun Amount:&nbsp;
                            <input id="donationCountryBreakdownMinimunAmountForDisplay" type="text" value={this.state.donationCountryBreakdownMinimunAmountForDisplay}
                                onChange={this.onDonationCountryBreakdownMinimunAmountForDisplayChange}
                            /> )
                        </small>
                    </div>
                    <div className="card-body">
                        {this.getDonationCountryBreakdownChart()}
                    </div>
                </div>



                {this.renderDonationSentToEndpointActivitySummaryTable()}
                <br /><br />
                {this.renderDonationEnqueuedActivitySummaryTable()}                
                <br /><br />
                {this.renderDonationProcessedActivitySummaryTable()}
                <br /><br />
                {this.renderDonationInfoActivitySummaryTable()}
                <br /><br />
                {this.renderDonationErrorsActivitySummaryTable()}
                <br /><br />

                {/*
                {this.renderDashboardResourceActivitySummaryTable()}
                <br /><br />
                */}
                  
            </div>
        );
    }
}

/*

                          <p className="card-text">With supporting text below as a natural lead-in to additional content.</p>
 */
